using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace Mapify.Patches
{
    /// <summary>
    ///     Pauses loading our custom map, and the rest of the game, until it's ready.
    /// </summary>
    /// <seealso cref="DisplayLoadingInfo_Start_Patch" />
    /// <seealso cref="MapLoader.LoadMap" />
    [HarmonyPatch(typeof(WorldStreamingInit), "Awake")]
    public static class WorldStreamingInit_Awake_Patch
    {
        public static bool CanLoad = false;
        public static bool CanInitialize = false;

        private static bool Prefix(WorldStreamingInit __instance)
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

    /// <summary>
    ///     Sets custom loading percentages.
    ///     This allows us to show our own during loading while keeping it in numerical order.
    ///     <br />
    ///     <br />
    ///     Also prevents the the WorldStreamingInit's vegetationStudioPrefab from being added to the scene"/>
    /// </summary>
    [HarmonyPatch(typeof(WorldStreamingInit), "LoadingRoutine", MethodType.Enumerator)]
    public static class WorldStreamingInit_LoadingRoutine_Patch
    {
        private static readonly Dictionary<string, float> loadingPercentages = new Dictionary<string, float> {
            { "creating player", 12.0f },
            { "loading savegame", 14.0f },
            { "<color=\"yellow\">loading savegame failed, trying with backup savegame</color>", 14.0f },
            { "<color=\"red\">loading savegame failed, using empty savegame</color>", 14.0f }
        };

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
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

    /// <summary>
    ///     Part of dynamic loading percentages.
    ///     Prevents the same message from being logged to console twice.
    /// </summary>
    /// <seealso cref="DisplayLoadingInfo_OnLoadingStatusChanged_Patch" />
    [HarmonyPatch(typeof(WorldStreamingInit), "Info")]
    public static class WorldStreamingInit_Info_Patch
    {
        private static readonly FieldInfo WorldStreamingInit_Field_logger = AccessTools.DeclaredField(typeof(WorldStreamingInit), "logger");
        private static string lastMessage;

        private static bool Prefix(WorldStreamingInit __instance, string msg, out bool __state)
        {
            Logger logger = (Logger)WorldStreamingInit_Field_logger.GetValue(__instance);
            __state = logger.logEnabled;
            logger.logEnabled = lastMessage != msg;
            lastMessage = msg;
            return true;
        }

        private static void Postfix(WorldStreamingInit __instance, bool __state)
        {
            Logger logger = (Logger)WorldStreamingInit_Field_logger.GetValue(__instance);
            logger.logEnabled = __state;
        }
    }
}
