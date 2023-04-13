using System.Reflection;
using HarmonyLib;
using TMPro;

namespace Mapify.Patches
{
    [HarmonyPatch(typeof(DisplayLoadingInfo), "Start")]
    public static class DisplayLoadingInfo_Start_Patch
    {
        private static void Postfix()
        {
            WorldStreamingInit_Awake_Patch.CanLoad = true;
        }
    }

    [HarmonyPatch(typeof(DisplayLoadingInfo), "OnLoadingStatusChanged")]
    public static class DisplayLoadingInfo_OnLoadingStatusChanged_Patch
    {
        private static readonly FieldInfo DisplayLoadingInfo_Field_loadProgressTMP = AccessTools.DeclaredField(typeof(DisplayLoadingInfo), "loadProgressTMP");
        private static readonly FieldInfo DisplayLoadingInfo_Field_percentageLoadedTMP = AccessTools.DeclaredField(typeof(DisplayLoadingInfo), "percentageLoadedTMP");

        public static string what;
        private static string lastMessage;

        private static bool Prefix(DisplayLoadingInfo __instance, string message, bool isError, float percentageLoaded)
        {
            if (lastMessage != message)
            {
                TextMeshProUGUI loadProgressTMP = (TextMeshProUGUI)DisplayLoadingInfo_Field_loadProgressTMP.GetValue(__instance);
                loadProgressTMP.text += "\n" + (isError ? "[error]" : "") + message;
                lastMessage = message;
            }

            TextMeshProUGUI percentageLoadedTMP = (TextMeshProUGUI)DisplayLoadingInfo_Field_percentageLoadedTMP.GetValue(__instance);
            percentageLoadedTMP.text = $"loading{(string.IsNullOrWhiteSpace(what) ? "" : $" {what}")}, please wait... {percentageLoaded}%";

            if (!Bootstrap.bootstrapped)
                return false;
            SingletonBehaviour<LoadingScreenManager>.Instance.UpdateProgress(percentageLoaded / 100f);
            return false;
        }
    }
}
