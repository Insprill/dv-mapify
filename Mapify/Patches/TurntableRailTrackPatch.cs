using HarmonyLib;
using Mapify.Utils;

namespace Mapify.Patches
{
    [HarmonyPatch(typeof(TurntableRailTrack), nameof(TurntableRailTrack.RotateToTargetRotation))]
    public class TurntableRailTrack_RotateToTargetRotation_Patch
    {
        public static void Postfix(TurntableRailTrack __instance)
        {
            RailwayMeshUpdater.UpdateTrack(__instance.Track);
        }
    }
}
