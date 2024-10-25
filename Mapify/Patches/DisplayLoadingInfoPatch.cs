using HarmonyLib;

namespace Mapify.Patches
{
    /// <summary>
    ///     Signals that the loading screen is being shown, and loading can proceed.
    /// </summary>
    /// <seealso cref="WorldStreamingInit_Awake_Patch" />
    [HarmonyPatch(typeof(DisplayLoadingInfo), nameof(DisplayLoadingInfo.Start))]
    public static class DisplayLoadingInfo_Start_Patch
    {
        private static void Postfix()
        {
            WorldStreamingInit_Awake_Patch.CanLoad = true;
        }
    }

    [HarmonyPatch(typeof(DisplayLoadingInfo), nameof(DisplayLoadingInfo.OnDestroy))]
    public static class DisplayLoadingInfo_OnDestroy_Patch
    {
        private static void Postfix()
        {
            WorldStreamingInit_Awake_Patch.CanLoad = false;
        }
    }

    /// <summary>
    ///     Part of dynamic loading percentages.
    ///     Prevents the same message from being listed twice, while also allowing
    ///     for something to be specified before the percentage.
    /// </summary>
    [HarmonyPatch(typeof(DisplayLoadingInfo), nameof(DisplayLoadingInfo.OnLoadingStatusChanged))]
    public static class DisplayLoadingInfo_OnLoadingStatusChanged_Patch
    {
        public static string what;
        private static string lastMessage;

        private static bool Prefix(DisplayLoadingInfo __instance, string message, bool isError, ref float percentageLoaded)
        {
            if (lastMessage != message)
            {
                __instance.loadProgressTMP.text += $"\n{(isError ? "[error]" : "")}{message}";
                lastMessage = message;
            }

            string formattedWhat = string.IsNullOrWhiteSpace(what) ? "" : $" {what}";
            __instance.percentageLoadedTMP.text = Locale.Get(Locale.LOADING__PLEASE_WAIT, formattedWhat, percentageLoaded.ToString("F0"));

            if (!Bootstrap.bootstrapped)
                return false;
            LoadingScreenManager.Instance.UpdateProgress(percentageLoaded / 100f);
            return false;
        }
    }
}
