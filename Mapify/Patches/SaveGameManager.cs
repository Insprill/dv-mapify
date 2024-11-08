using System.IO;
using DV.Common;
using HarmonyLib;
using Mapify.BuildMode;
using Mapify.Map;

namespace Mapify.Patches
{
    /// <summary>
    /// save the game -> save placed assets
    /// </summary>
    [HarmonyPatch (typeof(SaveGameManager), nameof(SaveGameManager.Save))]
    public static class SaveGameManager_Save_Patch
    {
        private static void Postfix(ISaveGame __result)
        {
            // result is null if saving is not allowed at this point
            if(__result == null){ return; }

            string xmlPath;
            if (Maps.IsDefaultMap)
            {
                xmlPath = BuildModeClass.GetDefaultMapXMLPath();
            }
            else
            {
                xmlPath = Path.Combine(
                    Maps.GetDirectory(Maps.LoadedMap),
                    Constants.PLACED_ASSETS_XML
                );
            }

            BuildModeClass.Instance.SavePlacedAssets(xmlPath);
        }
    }
}
