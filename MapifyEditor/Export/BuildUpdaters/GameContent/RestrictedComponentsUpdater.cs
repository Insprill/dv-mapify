#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Mapify.Editor.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mapify.Editor.StateUpdaters
{
    public class RestrictedComponentsUpdater : BuildUpdater
    {
        protected override void Update(Scenes scenes)
        {
            foreach (Scene scene in scenes.AllScenes())
            {
                IEnumerable<Behaviour> behaviours = scene.GetAllComponents<Camera>()
                    .Cast<Behaviour>()
                    .Concat(scene.GetAllComponents<AudioListener>())
                    .Concat(scene.GetAllComponents<Light>());
                foreach (Behaviour behaviour in behaviours)
                {
                    if (behaviour is Light light && light.type != LightType.Directional)
                        continue;
                    behaviour.tag = "EditorOnly";
                }
            }
        }
    }
}
#endif
