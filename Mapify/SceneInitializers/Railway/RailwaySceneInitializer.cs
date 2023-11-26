using System.Linq;
using Mapify.Editor;
using Mapify.Utils;
using Mapify.Editor.Utils;
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
            //switches get duplicated if we don't do this. This is a buǵ but I can't figure out the cause of the buǵ.
            foreach (var switchComponent in scene.GetAllComponents<Switch>())
            {
                var switchObject = switchComponent.gameObject;
                Mapify.LogWarning($"Deleting switch '{switchObject.name}' to avoid duplication");
                GameObject.Destroy(switchObject);
            }

            Transform railwayParent = WorldMover.Instance.NewChild("[railway]").transform;
            foreach (Transform transform in scene.GetRootGameObjects().Select(go => go.transform))
                transform.SetParent(railwayParent);
            base.Run();
        }
    }
}
