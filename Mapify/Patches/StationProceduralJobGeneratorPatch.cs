using HarmonyLib;

namespace Mapify.Patches
{
    [HarmonyPatch(typeof(StationProceduralJobGenerator), "GenerateBaseCargoTrainData")]
    public static class StationProceduralJobGenerator_GenerateBaseCargoTrainData_Patch
    {
        private static void Prefix(ref int minNumberOfCars, int maxNumberOfCars)
        {
            if (minNumberOfCars > maxNumberOfCars)
                minNumberOfCars = maxNumberOfCars;
        }
    }
}
