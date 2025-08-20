using DV;
using HarmonyLib;

namespace Mapify.Patches
{
    [HarmonyPatch(typeof(DerailedTrainCarIsKinematicHandler), nameof(DerailedTrainCarIsKinematicHandler.UpdateIsKinematicDependingOnLoadedCells))]
    public static class DerailedTrainCarIsKinematicHandler_UpdateIsKinematicDependingOnLoadedCells_Patch
    {
        private static bool Prefix(DerailedTrainCarIsKinematicHandler __instance)
        {
            __instance.train.rb.isKinematic = false;
            return false;
        }
    }
}
