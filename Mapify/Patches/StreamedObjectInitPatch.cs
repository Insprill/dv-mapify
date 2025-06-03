using System;
using System.Linq;
using HarmonyLib;
using Mapify.Editor;
using UnityEngine;

namespace Mapify.Patches
{
    /// <summary>
    ///     We can't reference the Streamer component from the editor module,
    ///     so we instead patch this method and put the functionality here.
    /// </summary>
    /// <seealso cref="SceneSplitManager" />
    [HarmonyPatch(typeof(StreamedObjectInit), nameof(StreamedObjectInit.Start))]
    public static class StreamedObjectInitPatch
    {
        private static Streamer[] streamers;

        private static void Postfix(StreamedObjectInit __instance)
        {
            if (streamers == null)
            {
                streamers = GameObject.FindGameObjectsWithTag(Streamer.STREAMERTAG)
                    .Select(go => go.GetComponent<Streamer>())
                    .Where(s => s != null)
                    .ToArray();
            }

            Streamer streamer = Array.Find(streamers, s => s.sceneCollection.names.Contains(__instance.sceneName));
            if (streamer == null)
            {
                Mapify.LogError($"Failed to find streamer for scene {__instance.sceneName}");
                return;
            }

            streamer.AddSceneGO(__instance.sceneName, __instance.gameObject);
        }

        public static void ResetStreamers()
        {
            streamers = null;
        }
    }
}
