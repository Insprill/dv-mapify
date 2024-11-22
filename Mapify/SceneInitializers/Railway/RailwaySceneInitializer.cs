using System.Linq;
using DV;
using Mapify.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mapify.SceneInitializers.Railway
{
    public class RailwaySceneInitializer : SceneInitializer
    {
        public RailwaySceneInitializer(Scene scene) : base(scene)
        { }

        public override void Run()
        {
            Transform railwayParent = WorldMover.OriginShiftParent.gameObject.NewChild(WorldData.RAILWAY_ROOT).transform;
            WorldData.Instance._trackRootParent = railwayParent;

            foreach (Transform transform in scene.GetRootGameObjects().Select(go => go.transform))
                transform.SetParent(railwayParent);
            base.Run();
        }
    }
}
