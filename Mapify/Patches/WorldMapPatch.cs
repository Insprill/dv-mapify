using HarmonyLib;
using Mapify.Map;
using Mapify.SceneInitializers.GameContent;

namespace Mapify.Patches
{
    [HarmonyPatch(typeof(WorldMap), "Awake")]
    public static class WorldMap_Awake_Patch
    {
        private static void Postfix(WorldMap __instance)
        {
            if (!Maps.IsDefaultMap) {
                WorldMapSetup.UpdateMap(__instance.transform);
            }
        }
    }
}
