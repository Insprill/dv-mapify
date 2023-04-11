using System.Collections.Generic;
using Mapify.Editor;
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

        private static IEnumerator<(VanillaAsset, GameObject)> ToSave(GameObject gameObject)
        {
            if (gameObject.name != "[railway]")
                yield break;

            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                GameObject child = gameObject.transform.GetChild(i).gameObject;
                string name = child.name;
                switch (name)
                {
                    case "Turntable Track":
                        yield return (VanillaAsset.TurntableTrack, child);
                        continue;
                    case "TurntableControlPanel":
                        yield return (VanillaAsset.TurntableControlPanel, child);
                        // The control panel loses this reference for whatever reason, so we have to copy it too.
                        yield return (VanillaAsset.TurntableRotateLayered, child.GetComponent<TurntableController>().turntableRotateLayered.gameObject);
                        continue;
                    case "term-buffer":
                        yield return (VanillaAsset.BufferStop, child);
                        continue;
                }

                if (child.transform.rotation.x != 0.0f || child.transform.rotation.z != 0.0f)
                    continue;
                switch (name)
                {
                    case "junc-left":
                        CleanupSwitch(child);
                        yield return (VanillaAsset.SwitchLeft, child);
                        continue;
                    case "junc-right":
                        CleanupSwitch(child);
                        yield return (VanillaAsset.SwitchRight, child);
                        continue;
                    case "junc-left-outer-sign":
                        CleanupSwitch(child);
                        yield return (VanillaAsset.SwitchLeftOuterSign, child);
                        continue;
                    case "junc-right-outer-sign":
                        CleanupSwitch(child);
                        yield return (VanillaAsset.SwitchRightOuterSign, child);
                        continue;
                }
            }
        }

        private static void CleanupSwitch(GameObject gameObject)
        {
            foreach (Junction junction in gameObject.GetComponentsInChildren<Junction>()) Object.Destroy(junction);
            foreach (BezierCurve curve in gameObject.GetComponentsInChildren<BezierCurve>()) GameObject.Destroy(curve.gameObject);
        }
    }
}
