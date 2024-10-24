using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using DV.TerrainSystem;
using HarmonyLib;
using Mapify.Map;
using UnityEngine;

namespace Mapify.Patches
{
    /// <summary>
    ///     Prevents TerrainsInfoFromAssetBundle's constructor from trying to load vanilla's info AssetBundle.
    /// </summary>
    /// <seealso cref="TerrainsInfoAssetBundleLoader_Constructor_Patch" />
    [HarmonyPatch(typeof(TerrainsInfoAssetBundleLoader), MethodType.Constructor, typeof(string), typeof(Func<IEnumerator, Coroutine>))]
    public static class TerrainsInfoAssetBundleLoader_Constructor_Transpiler
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);

            for (int i = 0; i < codes.Count; i++)
                if (codes[i].opcode == OpCodes.Stfld && codes[i].operand.ToString().Contains("assBunInfo"))
                {
                    // Don't set assBunInfo in the constructor since the AssetBundle it tries to load doesn't exist.
                    codes.RemoveRange(i - 12, 13);
                    break;
                }

            return codes.AsEnumerable();
        }
    }

    /// <summary>
    ///     Sets our own TerrainsInfoFromAssetBundle.
    ///     We must create a new one since we transpiled out the original set in the constructor.
    /// </summary>
    /// <seealso cref="TerrainsInfoAssetBundleLoader_Constructor_Transpiler" />
    [HarmonyPatch(typeof(TerrainsInfoAssetBundleLoader), MethodType.Constructor, typeof(string), typeof(Func<IEnumerator, Coroutine>))]
    public static class TerrainsInfoAssetBundleLoader_Constructor_Patch
    {
        private static void Postfix(TerrainsInfoAssetBundleLoader __instance, string worldName)
        {
            TerrainsInfoFromAssetBundle bundle;
            if (Maps.IsDefaultMap)
            {
                // The line we removed in the transpiler
                bundle = (TerrainsInfoFromAssetBundle)AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, worldName, "info")).LoadAllAssets()[0];
            }
            else
            {
                // Set our own terrain info
                bundle = ScriptableObject.CreateInstance<TerrainsInfoFromAssetBundle>();
                bundle.terrainSizeInWorld = Maps.LoadedMap.terrainSize;
                bundle.numberOfTerrains = Maps.LoadedMap.terrainCount;
            }

            __instance.assBunInfo = bundle;
        }
    }

    /// <summary>
    ///     Redirects requests to load new terrain to our map installation location.
    /// </summary>
    [HarmonyPatch(typeof(TerrainsInfoAssetBundleLoader), nameof(TerrainsInfoAssetBundleLoader.GetAssetBundleFilePath))]
    public static class TerrainsInfoAssetBundleLoader_GetAssetBundleFilePath_Patch
    {
        private static bool Prefix(TerrainsInfoAssetBundleLoader __instance, Vector2Int coord, ref string __result)
        {
            if (Maps.IsDefaultMap)
                return true;
            __result = Maps.GetMapAsset($"terraindata_{coord.y * __instance.TerrainsPerAxis + coord.x}");
            return false;
        }
    }
}
