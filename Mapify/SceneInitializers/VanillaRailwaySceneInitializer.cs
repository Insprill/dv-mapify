using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mapify.SceneInitializers
{
    public static class VanillaRailwaySceneInitializer
    {
        private static readonly Dictionary<string, GameObject> switchPrefabs = new Dictionary<string, GameObject>(4);

        public static GameObject GetSwitchPrefab(string name)
        {
            return switchPrefabs[name];
        }

        public static void SceneLoaded(Scene scene)
        {
            CopyDefaultAssets(scene.path);
        }

        private static void CopyDefaultAssets(string scenePath)
        {
            Scene scene = SceneManager.GetSceneByPath(scenePath);
            if (!scene.isLoaded)
            {
                Main.Logger.Error($"Default scene {scenePath} isn't loaded!");
                return;
            }

            GameObject[] gameObjects = scene.GetRootGameObjects();
            GameObject railwayRoot = null;
            foreach (GameObject rootObject in gameObjects)
            {
                rootObject.SetActive(false);
                if (rootObject.name != "[railway]") continue;
                railwayRoot = rootObject;
                Main.Logger.Log("Moving [railway] to the active scene");
                SceneManager.MoveGameObjectToScene(rootObject, SceneManager.GetActiveScene());
                break;
            }

            Main.Logger.Log("Unloading default railway scene");
            // cope
#pragma warning disable CS0618
            SceneManager.UnloadScene(scene);
#pragma warning restore CS0618

            if (railwayRoot == null)
            {
                Main.Logger.Error("Failed to find [railway]!");
                return;
            }

            for (int i = 0; i < railwayRoot.transform.childCount; i++)
            {
                GameObject gameObject = railwayRoot.transform.GetChild(i).gameObject;
                string name = gameObject.name;
                switch (name)
                {
                    case "junc-left":
                    case "junc-right":
                    case "junc-left-outer-sign":
                    case "junc-right-outer-sign":
                        if (switchPrefabs.ContainsKey(name) || gameObject.transform.rotation.x != 0.0f || gameObject.transform.rotation.z != 0.0f) continue;
                        Main.Logger.Log($"Found {name}");
                        CleanupSwitch(gameObject);
                        switchPrefabs[name] = gameObject;
                        break;
                }
            }

            GameObject.DestroyImmediate(railwayRoot);
        }

        private static void CleanupSwitch(GameObject gameObject)
        {
            gameObject.transform.SetParent(null);
            gameObject.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            foreach (Junction junction in gameObject.GetComponentsInChildren<Junction>()) Object.Destroy(junction);
            foreach (BezierCurve curve in gameObject.GetComponentsInChildren<BezierCurve>()) GameObject.Destroy(curve.gameObject);
            gameObject.SetActive(false);
        }
    }
}
