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
                Mesh mesh = filter.sharedMesh;
                if (mesh == null) continue;
                switch (mesh.name)
                {
                    case "TurntablePit":
                        yield return (VanillaAsset.TurntablePit, filter.transform.parent.gameObject);
                        break;
                    case "ItemShop":
                        yield return (VanillaAsset.StoreMesh, filter.transform.parent.gameObject);
                        break;
                }
            }
        }
    }
}
