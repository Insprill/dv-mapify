using System;
using System.Collections.Generic;
using System.Linq;
using DV.Utils;
using HarmonyLib;
using Mapify.Editor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Mapify
{
    public abstract class AssetCopier
    {
        private static readonly Dictionary<VanillaAsset, GameObject> prefabs = new Dictionary<VanillaAsset, GameObject>(Enum.GetValues(typeof(VanillaAsset)).Length);

        public static IEnumerable<VanillaAsset> InstantiatableAssets => prefabs.Keys;

        public static GameObject Instantiate(VanillaAsset asset, bool active = true, bool originShift = true)
        {
            Mapify.LogDebug(() => "Instantiating asset: " + asset);
            var gameObject = GameObject.Instantiate(prefabs[asset], originShift ? WorldMover.OriginShiftParent : null);
            gameObject.SetActive(active);
            return gameObject;
        }

        public static void Cleanup()
        {
            foreach (GameObject go in prefabs.Values)
                Object.Destroy(go);
            prefabs.Clear();
        }

        protected abstract IEnumerator<(VanillaAsset, GameObject)> ToSave(GameObject gameObject);

        public void CopyAssets(Scene scene)
        {
            if (!scene.isLoaded)
                throw new InvalidOperationException($"Tried to copy vanilla assets from {scene.name} but it isn't loaded!");

            Mapify.LogDebugExtreme(() => $"Copying default assets from vanilla scene {scene.name}");

            GameObject[] rootObjects = scene.GetRootGameObjects();
            foreach (GameObject rootObject in rootObjects)
            {
                rootObject.SetActive(false);
                SceneManager.MoveGameObjectToScene(rootObject, SceneManager.GetActiveScene());

                IEnumerator<(VanillaAsset, GameObject)> enumerator = ToSave(rootObject);
                while (enumerator.MoveNext())
                {
                    (VanillaAsset vanillaAsset, GameObject gameObject) = enumerator.Current;
                    if (gameObject == null)
                    {
                        Mapify.LogError($"Failed to find game object for {vanillaAsset}! This MUST be fixed!");
                        continue;
                    }

                    if (prefabs.ContainsKey(vanillaAsset)) continue;
                    foreach (__SingletonBehaviourBase behaviour in gameObject.GetComponentsInChildren<__SingletonBehaviourBase>())
                        AccessTools.Method(behaviour.GetType(), "OnDestroy")?.Invoke(behaviour, null);
                    gameObject.SetActive(false);
                    gameObject.transform.SetParent(null);
                    gameObject.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                    prefabs.Add(vanillaAsset, gameObject);
                }

                if (!prefabs.Values.Contains(rootObject))
                    Object.Destroy(rootObject);
            }

            Mapify.LogDebugExtreme(() => $"Unloading vanilla scene {scene.name}");
            SceneManager.UnloadSceneAsync(scene);
        }
    }
}
