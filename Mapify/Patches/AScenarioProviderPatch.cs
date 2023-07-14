using System.Linq;
using DV.Common;
using DV.UI;
using HarmonyLib;
using Mapify.Editor;
using Mapify.Map;
using Mapify.Utils;

namespace Mapify.Patches
{
    [HarmonyPatch(typeof(AScenarioProvider), nameof(AScenarioProvider.CanStartNewSession))]
    public static class AScenarioProvider_CanStartNewSession_Patch
    {
        private static bool Prefix(IGameSession session, ref (bool canRun, string reasonLocKey) __result)
        {
            BasicMapInfo basicMapInfo = session.GameData.GetBasicMapInfo();
            if (Maps.AllMapNames.Contains(basicMapInfo.name))
                return true;
            __result = (false, Locale.LAUNCHER__SESSION_MAP_NOT_INSTALLED);
            return false;
        }
    }
}
