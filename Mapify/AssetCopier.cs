using System;
using System.Collections.Generic;
using System.Linq;
using Mapify.Editor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Mapify
{
    public static class AssetCopier
    {
        private static readonly Dictionary<VanillaAsset, GameObject> prefabs = new Dictionary<VanillaAsset, GameObject>(Enum.GetValues(typeof(VanillaAsset)).Length);

        public static IEnumerable<VanillaAsset> InstantiatableAssets => prefabs.Keys;

        public static GameObject Instantiate(VanillaAsset asset, bool originShift = true, bool active = true)
        {
            GameObject go = GameObject.Instantiate(prefabs[asset], originShift ? WorldMover.Instance.originShiftParent : null);
            go.SetActive(active);
            return go;
        }

        public static void CopyDefaultAssets(Scene scene, ToSave func)
        {
            if (!scene.isLoaded)
            {
                Main.LogError($"Tried to copy vanilla assets from {scene.name} but it isn't loaded!");
                return;
            }

            Main.LogDebug($"Copying default assets from vanilla scene {scene.name}");

            GameObject[] rootObjects = scene.GetRootGameObjects();
            foreach (GameObject rootObject in rootObjects)
            {
                rootObject.SetActive(false);
                SceneManager.MoveGameObjectToScene(rootObject, SceneManager.GetActiveScene());

                IEnumerator<(VanillaAsset, GameObject)> enumerator = func.Invoke(rootObject);
                while (enumerator.MoveNext())
                {
                    (VanillaAsset vanillaAsset, GameObject gameObject) = enumerator.Current;
                    if (prefabs.ContainsKey(vanillaAsset)) continue;
                    gameObject.SetActive(false);
                    gameObject.transform.SetParent(null);
                    gameObject.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                    prefabs.Add(vanillaAsset, gameObject);
                }

                if (!prefabs.Values.Contains(rootObject))
                    Object.Destroy(rootObject);
            }

            Main.LogDebug($"Unloading vanilla scene {scene.name}");
            // cope
#pragma warning disable CS0618
            SceneManager.UnloadScene(scene);
#pragma warning restore CS0618
        }

        public delegate IEnumerator<(VanillaAsset, GameObject)> ToSave(GameObject gameObject);
    }
}
