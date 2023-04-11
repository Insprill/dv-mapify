using System.Collections.Generic;
using Mapify.Editor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mapify.SceneInitializers
{
    public static class VanillaStreamerSceneInitializer
    {
        public static void SceneLoaded(Scene scene)
        {
            AssetCopier.CopyDefaultAssets(scene, ToSave);
        }

        private static IEnumerator<(VanillaAsset, GameObject)> ToSave(GameObject gameObject)
        {
            foreach (MeshFilter filter in gameObject.GetComponentsInChildren<MeshFilter>(true))
            {
                if (filter.sharedMesh.name != "TurntablePit") continue;
                yield return (VanillaAsset.TurntablePit, filter.transform.parent.gameObject);
                break;
            }
        }
    }
}
