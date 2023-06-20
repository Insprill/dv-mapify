using System;
using Mapify.Editor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Mapify.SceneInitializers.Railway
{
    [SceneSetupPriority(int.MaxValue)]
    public class TrackLodSetup : SceneSetup
    {
        public override void Run()
        {
            RailwayLodGenerator lodGenerator = new GameObject("Railway LOD Generator").AddComponent<RailwayLodGenerator>();
            // RailTrackRegistry#AllTracks causes issues here for some reason
            BaseType basedType = Array.Find(Object.FindObjectsOfType<RailTrack>(), rt => rt.baseType != null)?.baseType;
            if (basedType == null)
            {
                Mapify.LogError($"Failed to find a {nameof(BaseType)} to use for railway LOD generation!");
                return;
            }

            lodGenerator.profile = basedType.baseShape;
            GameObject ballastLodMaterialObject = AssetCopier.Instantiate(VanillaAsset.BallastLodMaterial);
            lodGenerator.mat = ballastLodMaterialObject.GetComponent<Renderer>().sharedMaterial;
            Object.Destroy(ballastLodMaterialObject);
        }
    }
}
