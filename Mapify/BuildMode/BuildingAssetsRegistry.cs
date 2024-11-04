﻿using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Mapify.BuildMode
{
    public static class BuildingAssetsRegistry
    {
        public static SortedDictionary<string, GameObject> Assets = new SortedDictionary<string, GameObject>();
        private static GameObject registryMainObject;
        private static List<string> searchedScenes = new List<string>();

        private static readonly Regex CLONE_PATTERN = new Regex(@"\(\d+\)$"); //matches 'blabla (1)' and not 'blabla'

        public static void OnSceneLoad(Scene scene, LoadSceneMode mode)
        {
            //TODO check if this scene is of the default game
            if (searchedScenes.Contains(scene.name))
            {
                return;
            }

            if (registryMainObject == null)
            {
                registryMainObject = new GameObject("[BuildingAssets]");
            }

            searchedScenes.Add(scene.name);
            Mapify.Log("RegisterAssets "+scene.name);

            foreach (var rootObject in scene.GetRootGameObjects())
            {
                foreach (var lod in rootObject.GetComponentsInChildren<LODGroup>(true))
                {
                    var originalObject = lod.gameObject;

                    //TODO Names of objects might not be unique. Is there a better way to determine whether 2 GameObjects are of the same asset?
                    //avoid duplicates
                    if (CLONE_PATTERN.IsMatch(originalObject.name) || Assets.ContainsKey(originalObject.name))
                    {
                        continue;
                    }
                    var copy = Object.Instantiate(originalObject, registryMainObject.transform);
                    copy.SetActive(false);
                    copy.name = originalObject.name;
                    Assets.Add(copy.name, copy);
                }
            }
        }

        public static void FinishRegistering()
        {
            Mapify.LogDebug(() => $"Assets({Assets.Count}):");

            foreach (var ass in Assets)
            {
                Mapify.LogDebug(() => ass.Value.name);
            }

            BuildModeClass.Instance.SetupAssetSelectMenu();
        }

        public static void CleanUp()
        {
            foreach (var ass in Assets)
            {
                Object.Destroy(ass.Value);
            }

            Assets.Clear();
            Object.Destroy(registryMainObject);
        }
    }
}
