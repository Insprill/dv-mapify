using System.Collections.Generic;
using Mapify.Editor;
using Mapify.Editor.Utils;
using UnityEngine;

namespace Mapify.SceneInitializers.Vanilla.Streaming
{
    public class StreamerCopier : AssetCopier
    {
        protected override IEnumerator<(VanillaAsset, GameObject)> ToSave(GameObject gameObject)
        {
            foreach (MeshFilter filter in gameObject.GetComponentsInChildren<MeshFilter>(true))
            {
                Mesh mesh = filter.sharedMesh;
                if (mesh == null) continue;
                switch (mesh.name)
                {
                    case "TurntablePit": {
                        Transform parent = Object.Instantiate(filter.transform.parent.gameObject).transform;
                        parent.gameObject.SetActive(false);
                        Object.Destroy(parent.FindChildByName("TurntableControlHouse").gameObject);
                        Object.Destroy(parent.FindChildByName("TurntableControlHouse_LOD1").gameObject);
                        Object.Destroy(parent.FindChildByName("TurntableControlHouse_ShadowCaster").gameObject);
                        foreach (Transform t in parent)
                            t.localPosition = Vector3.zero;
                        yield return (VanillaAsset.TurntablePit, parent.gameObject);
                        break;
                    }
                    case "TurntableControlHouse": {
                        Transform parent = Object.Instantiate(filter.transform.parent.gameObject).transform;
                        parent.gameObject.SetActive(false);
                        Object.Destroy(parent.FindChildByName("TurntablePit").gameObject);
                        Object.Destroy(parent.FindChildByName("TurntablePit_LOD1").gameObject);
                        Object.Destroy(parent.FindChildByName("TurntablePit_ShadowCaster").gameObject);
                        foreach (Transform t in parent)
                            t.localPosition = Vector3.zero;
                        yield return (VanillaAsset.TurntableControlShed, parent.gameObject);
                        break;
                    }
                    case "ItemShop":
                        yield return (VanillaAsset.StoreMesh, filter.transform.parent.gameObject);
                        break;
                }
            }

            foreach (Renderer renderer in gameObject.GetComponentsInChildren<Renderer>(true))
            {
                Material material = renderer.sharedMaterial;
                if (material == null) continue;
                switch (material.name)
                {
                    case "BallastLOD":
                        yield return (VanillaAsset.BallastLodMaterial, renderer.gameObject);
                        break;
                }
            }
        }
    }
}
