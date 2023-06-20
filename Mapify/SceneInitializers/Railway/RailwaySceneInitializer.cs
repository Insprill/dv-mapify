using System.Linq;
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
            Transform railwayParent = WorldMover.Instance.NewChild("[railway]").transform;
            foreach (Transform transform in scene.GetRootGameObjects().Select(go => go.transform))
                transform.SetParent(railwayParent);
            base.Run();
        }
    }
}
