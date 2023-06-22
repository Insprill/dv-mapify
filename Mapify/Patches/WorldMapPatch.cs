using HarmonyLib;
using Mapify.Map;
using Mapify.SceneInitializers.GameContent;

namespace Mapify.Patches
{
    [HarmonyPatch(typeof(WorldMap), "Awake")]
    public static class WorldMap_Awake_Patch
    {
        private static bool modified;

        private static void Postfix(WorldMap __instance)
        {
            if (Maps.IsDefaultMap)
                return;
            if (modified) return;
            WorldMapSetup.UpdateMap(__instance.transform);
            modified = true;
        }
    }
}
