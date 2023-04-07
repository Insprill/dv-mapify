using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace Mapify.Patches
{
    [HarmonyPatch(typeof(WorldStreamingInit), "Awake")]
    public static class WorldStreamingInit_Awake_Patch
    {
        public static bool CanLoad = false;
        public static bool CanInitialize = false;

        public static bool Prefix(WorldStreamingInit __instance)
        {
            __instance.StartCoroutine(WaitForLoadingScreen());
            return false;
        }

        private static IEnumerator WaitForLoadingScreen()
        {
            WorldStreamingInit wsi = SingletonBehaviour<WorldStreamingInit>.Instance;
            yield return new WaitUntil(() => CanLoad);
            wsi.StartCoroutine(MapLoader.LoadMap());
            yield return new WaitUntil(() => CanInitialize);
            wsi.StartCoroutine("LoadingRoutine");
        }
    }

    [HarmonyPatch(typeof(WorldStreamingInit), "LoadingRoutine", MethodType.Enumerator)]
    public static class WorldStreamingInit_LoadingRoutine_Patch
    {
        private static readonly Dictionary<string, float> loadingPercentages = new Dictionary<string, float> {
            { "creating player", 12.0f },
            { "loading savegame", 14.0f },
            { "<color=\"yellow\">loading savegame failed, trying with backup savegame</color>", 14.0f },
            { "<color=\"red\">loading savegame failed, using empty savegame</color>", 14.0f }
        };

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);

            for (int i = 0; i < codes.Count; i++)
                if (codes[i].opcode == OpCodes.Ldstr)
                {
                    if (!(codes[i].operand is string operand))
                        continue;

                    // Don't add the vegetationStudioPrefab
                    if (operand == "loading terrain")
                    {
                        codes.RemoveRange(i - 9, 8);
                        continue;
                    }

                    if (loadingPercentages.TryGetValue(operand, out float newPercentage))
                        codes[i + 1].operand = newPercentage;
                }

            return codes.AsEnumerable();
        }
    }
}
