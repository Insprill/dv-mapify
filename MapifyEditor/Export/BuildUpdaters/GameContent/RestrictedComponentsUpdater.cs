using System.Collections.Generic;
using System.Linq;
using Mapify.Editor.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mapify.Editor.StateUpdaters
{
    public class RestrictedComponentsUpdater : BuildUpdater
    {
        private Dictionary<Behaviour, bool> components;

        protected override void Update(Scenes scenes)
        {
            components = new Dictionary<Behaviour, bool>();
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
                    components.Add(behaviour, behaviour.enabled);
                    behaviour.enabled = false;
                }
            }
        }

        protected override void Cleanup(Scenes scenes)
        {
            foreach (KeyValuePair<Behaviour, bool> pair in components)
                pair.Key.enabled = pair.Value;
        }
    }
}
