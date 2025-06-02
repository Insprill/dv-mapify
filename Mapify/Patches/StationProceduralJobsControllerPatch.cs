using HarmonyLib;
using Mapify.Components;
using Mapify.Map;

namespace Mapify.Patches
{
    //The base game has a limit of 30 job generation attempts per station. These patches remove this limitation and instead base the attempts count on the stations job capacity.

    [HarmonyPatch(typeof(StationProceduralJobsController), nameof(StationProceduralJobsController.Awake))]
    public static class StationProceduralJobsController_Awake_Patch
    {
        private static void Postfix(StationProceduralJobsController __instance)
        {
            if(Maps.IsDefaultMap) return;

            var customJobsController = __instance.gameObject.AddComponent<CustomJobsController>();
            customJobsController.Setup(__instance);
        }
    }

    [HarmonyPatch(typeof(StationProceduralJobsController), nameof(StationProceduralJobsController.TryToGenerateJobs))]
    public static class StationProceduralJobsController_TryToGenerateJobs_Patch
    {
        private static bool Prefix(StationProceduralJobsController __instance)
        {
            if(Maps.IsDefaultMap) return true;

            var customJobsController = __instance.gameObject.GetComponent<CustomJobsController>();

            __instance.StopJobGeneration();
            __instance.generationCoro = __instance.StartCoroutine(customJobsController.GenerateProceduralJobsCoro());

            return false;
        }
    }
}
