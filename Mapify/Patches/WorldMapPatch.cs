using HarmonyLib;
using Mapify.SceneInitializers.GameContent;

namespace Mapify.Patches
{
    [HarmonyPatch(typeof(WorldMap), "Awake")]
    public static class WorldMap_Awake_Patch
    {
        private static void Postfix(WorldMap __instance)
        {
            WorldMapSetup.UpdateMap(__instance.transform);
        }
    }
}
