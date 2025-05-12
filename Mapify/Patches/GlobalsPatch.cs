using DV;
using HarmonyLib;
using Mapify.BuildMode;
using UnityEngine.SceneManagement;

namespace Mapify.Patches
{
    [HarmonyPatch(typeof(Globals), nameof(Globals.G), MethodType.Getter)]
    public class GlobalsPatch
    {
        private static bool done = false;

        private static void Prefix()
        {
            if (done) { return; }
            SceneManager.sceneLoaded += BuildingAssetsRegistry.OnSceneLoad;
            done = true;
        }
    }
}
