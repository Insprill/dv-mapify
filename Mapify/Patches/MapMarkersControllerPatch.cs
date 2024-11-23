using HarmonyLib;
using Mapify.Editor.Utils;
using Mapify.Map;
using Mapify.SceneInitializers.GameContent;

namespace Mapify.Patches
{
    [HarmonyPatch(typeof(MapMarkersController), nameof(MapMarkersController.Awake))]
    public static class MapMarkersController_Awake_Patch
    {
        private static void Postfix(MapMarkersController __instance)
        {
            if (!Maps.IsDefaultMap) {
                WorldMapSetup.UpdateMap(__instance.transform.FindChildByName("MapPaper"));
            }
        }
    }
}
