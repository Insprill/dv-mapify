using System.Collections;
using System.Collections.Generic;
using DV.Utils;
using Mapify.BuildMode;
using Mapify.Editor.Utils;
using Mapify.Patches;
using Mapify.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mapify.Map
{
    /// <summary>
    /// like MapLifeCycle but for the default map
    /// </summary>
    public class MapLifeCycleDV
    {
        private static bool loading;
        private static List<string> loadedSceneNames;

        /// <summary>
        /// Loads all scenes of the default map, copies the assets to BuildingAssetsRegistry, then unloads them again.
        /// </summary>
        public static IEnumerator FakeLoadMap()
        {
            Mapify.LogDebug(() => nameof(FakeLoadMap));
            BuildingAssetsRegistry.Setup();
            loading = true;

            // Load scenes for us to steal assets from
            MonoBehaviourDisablerPatch.DisableAll();

            // Register scene loaded hook
            SceneManager.sceneLoaded += OnSceneLoad;

            var wsi = SingletonBehaviour<WorldStreamingInit>.Instance;

            DisplayLoadingInfo_OnLoadingStatusChanged_Patch.what = "vanilla assets";

            var loadingInfo = Object.FindObjectOfType<DisplayLoadingInfo>();
            const string loadingMapLogMsg = "Loading Assets for BuildMode";

            loadingInfo.UpdateLoadingStatus(loadingMapLogMsg, 0);

            loadedSceneNames = new List<string>();
            yield return LoadScenes(wsi);
            yield return UnLoadScenes();

            DisplayLoadingInfo_OnLoadingStatusChanged_Patch.what = null;

            Mapify.Log("Vanilla scenes unloaded");
            MonoBehaviourDisablerPatch.EnableAll();

            WorldStreamingInit_Awake_Patch.CanInitialize = true;
            BuildingAssetsRegistry.FinishRegistering();
            loading = false;
        }

        private static IEnumerator LoadScenes(WorldStreamingInit wsi)
        {
            // Streaming scenes
            var sceneNames = wsi.transform.FindChildByName("[far]").GetComponent<Streamer>().sceneCollection.names;

            var operations = new List<AsyncOperation>();
            foreach (var name in sceneNames)
            {
                var nameWithExtension = name.Replace(".unity", "");
                operations.Add(SceneManager.LoadSceneAsync(nameWithExtension, LoadSceneMode.Additive));
                loadedSceneNames.Add(nameWithExtension);
            }

            bool doneLoading;
            do
            {
                doneLoading = true;
                foreach (var operation in operations)
                {
                    if (operation.isDone) continue;
                    doneLoading = false;
                    break;
                }
                yield return null;
            } while (!doneLoading);
        }

        private static IEnumerator UnLoadScenes()
        {
            foreach (var sceneName in loadedSceneNames)
            {
                yield return UnloadSceneCoroutine(sceneName);
            }
        }

        private static void OnSceneLoad(Scene scene, LoadSceneMode mode)
        {
            Mapify.LogDebug(() => $"{nameof(MapLifeCycleDV)}.{nameof(OnSceneLoad)}: {scene.name}");
            if (scene.buildIndex == (int)DVScenes.MainMenu)
            {
                CleanupOnMainMenu();
                return;
            }

            if (!loading)
            {
                return;
            }

            if (MapLifeCycle.VANILLA_STREAMER_SCENE_PATTERN.IsMatch(scene.name))
            {
                BuildingAssetsRegistry.RegisterAssets(scene);
            }
        }

        private static IEnumerator UnloadSceneCoroutine(string sceneName)
        {
            var operation = SceneManager.UnloadSceneAsync(sceneName);
            if (operation == null)
            {
                Mapify.LogError("UnloadSceneAsync returned null somehow");
                yield break;
            }

            while (!operation.isDone)
            {
                yield return null;
            }
        }

        private static void CleanupOnMainMenu()
        {
            SceneManager.sceneLoaded -= OnSceneLoad;
            BuildingAssetsRegistry.CleanUp();
            WorldStreamingInit_Awake_Patch.CanInitialize = false;
        }
    }
}
