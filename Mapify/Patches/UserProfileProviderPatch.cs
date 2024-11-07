using System.IO;
using DV.Common;
using DV.UI;
using DV.UserManagement;
using DV.Utils;
using HarmonyLib;
using Mapify.BuildMode;
using Mapify.Editor;
using Mapify.Map;

namespace Mapify.Patches
{
    /// <summary>
    /// save the game -> save placed assets
    /// </summary>
    [HarmonyPatch (typeof(UserProfileProvider), nameof(UserProfileProvider.SaveGame))]
    public static class UserProfileProviderPatch_SaveGame_Patch
    {
        private static void Postfix(ISaveGame __result)
        {
            //TODO separate save button?
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
