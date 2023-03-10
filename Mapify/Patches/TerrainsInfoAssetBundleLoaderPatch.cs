using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using DV.TerrainSystem;
using HarmonyLib;
using UnityEngine;

namespace Mapify.Patches
{
    [HarmonyPatch(typeof(TerrainsInfoAssetBundleLoader), MethodType.Constructor, typeof(string), typeof(Func<IEnumerator, Coroutine>))]
    public static class TerrainsInfoAssetBundleLoader_Constructor_Transpiler
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);

            for (int i = 0; i < codes.Count; i++)
                if (codes[i].opcode == OpCodes.Stfld && codes[i].operand.ToString().Contains("assBunInfo"))
                {
                    // Don't set assBunInfo in the constructor since the AssetBundle it tries to load doesn't exist.
                    codes.RemoveRange(i - 13, 13);
                    break;
                }

            return codes.AsEnumerable();
        }
    }

    [HarmonyPatch(typeof(TerrainsInfoAssetBundleLoader), MethodType.Constructor, typeof(string), typeof(Func<IEnumerator, Coroutine>))]
    public static class TerrainsInfoAssetBundleLoader_Constructor_Patch
    {
        private static readonly FieldInfo Field_assBunInfo = AccessTools.DeclaredField(typeof(TerrainsInfoAssetBundleLoader), "assBunInfo");

        public static void Postfix(TerrainsInfoAssetBundleLoader __instance)
        {
            // Set our own terrain info
            TerrainsInfoFromAssetBundle bundle = ScriptableObject.CreateInstance<TerrainsInfoFromAssetBundle>();
            bundle.terrainSizeInWorld = SingletonBehaviour<LevelInfo>.Instance.worldSize;
            bundle.numberOfTerrains = 1; //todo: set this when we have terrain chunking
            bundle.hasMicroSplatDiffuse = false;
            bundle.hasMicroSplatNormal = false;
            Field_assBunInfo.SetValue(__instance, bundle);
        }
    }

    [HarmonyPatch(typeof(TerrainsInfoAssetBundleLoader), "GetAssetBundleFilePath")]
    public static class TerrainsInfoAssetBundleLoader_GetAssetBundleFilePath_Patch
    {
        public static bool Prefix(TerrainsInfoAssetBundleLoader __instance, ref string __result)
        {
            __result = SingletonBehaviour<WorldStreamingInit>.Instance.terrainsScenePath;
            return false;
        }
    }
}
