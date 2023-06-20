using System;
using System.Collections.Generic;
using System.Linq;
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
            GameObject go = GameObject.Instantiate(prefabs[asset], originShift ? WorldMover.Instance.originShiftParent : null);
            go.SetActive(active);
            return go;
        }

        protected abstract IEnumerator<(VanillaAsset, GameObject)> ToSave(GameObject gameObject);

        public void CopyAssets(Scene scene)
        {
            if (!scene.isLoaded)
            {
                Mapify.LogError($"Tried to copy vanilla assets from {scene.name} but it isn't loaded!");
                return;
            }

            Mapify.LogDebug($"Copying default assets from vanilla scene {scene.name}");

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
                    gameObject.SetActive(false);
                    gameObject.transform.SetParent(null);
                    gameObject.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                    prefabs.Add(vanillaAsset, gameObject);
                }

                if (!prefabs.Values.Contains(rootObject))
                    Object.Destroy(rootObject);
            }

            Mapify.LogDebug($"Unloading vanilla scene {scene.name}");
            // cope
#pragma warning disable CS0618
            SceneManager.UnloadScene(scene);
#pragma warning restore CS0618
        }
    }
}
