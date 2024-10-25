using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mapify.SceneInitializers.GameContent
{
    public class GameContentSceneInitializer : SceneInitializer
    {
        public GameContentSceneInitializer(Scene scene) : base(scene)
        { }

        public override void Run()
        {
            foreach (Transform transform in scene.GetRootGameObjects().Select(go => go.transform))
                transform.SetParent(WorldMover.OriginShiftParent);
            base.Run();
        }
    }
}
