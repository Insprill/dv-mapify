#if UNITY_EDITOR
using System.Collections.Generic;
using Mapify.Editor.Utils;
using UnityEngine;

namespace Mapify.Editor.Validators
{
    public class LightsValidator : Validator
    {
        protected override IEnumerator<Result> Validate(Scenes scenes)
        {
            Light[] lights = scenes.streamingScene.GetAllComponents<Light>();
            foreach (Light light in lights)
                if (light.type == LightType.Directional)
                    yield return Result.Error($"You shouldn't have any Directional Lights in the {scenes.streamingScene.name} scene! It/they should go in the {scenes.gameContentScene.name}");
        }
    }
}
#endif
