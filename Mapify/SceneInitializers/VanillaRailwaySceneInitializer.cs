using System.Collections.Generic;
using Mapify.Editor;
using Mapify.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mapify.SceneInitializers
{
    public static class VanillaRailwaySceneInitializer
    {
        public static void SceneLoaded(Scene scene)
        {
            AssetCopier.CopyDefaultAssets(scene, ToSave);
        }

        private static Dictionary<VanillaAsset, GameObject> ToSave(GameObject gameObject)
        {
            Dictionary<VanillaAsset, GameObject> gameObjects = new Dictionary<VanillaAsset, GameObject>(4);
            if (gameObject.name != "[railway]")
                return gameObjects;

            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                GameObject child = gameObject.transform.GetChild(i).gameObject;
                if (child.transform.rotation.x != 0.0f || child.transform.rotation.z != 0.0f)
                    continue;
                string name = child.name;
                switch (name)
                {
                    case "junc-left":
                        CleanupSwitch(child);
                        gameObjects.TryAdd(VanillaAsset.SwitchLeft, child);
                        break;
                    case "junc-right":
                        CleanupSwitch(child);
                        gameObjects.TryAdd(VanillaAsset.SwitchRight, child);
                        break;
                    case "junc-left-outer-sign":
                        CleanupSwitch(child);
                        gameObjects.TryAdd(VanillaAsset.SwitchLeftOuterSign, child);
                        break;
                    case "junc-right-outer-sign":
                        CleanupSwitch(child);
                        gameObjects.TryAdd(VanillaAsset.SwitchRightOuterSign, child);
                        break;
                    case "term-buffer":
                        gameObjects.TryAdd(VanillaAsset.BufferStop, child);
                        break;
                }
            }

            return gameObjects;
        }

        private static void CleanupSwitch(GameObject gameObject)
        {
            foreach (Junction junction in gameObject.GetComponentsInChildren<Junction>()) Object.Destroy(junction);
            foreach (BezierCurve curve in gameObject.GetComponentsInChildren<BezierCurve>()) GameObject.Destroy(curve.gameObject);
        }
    }
}
