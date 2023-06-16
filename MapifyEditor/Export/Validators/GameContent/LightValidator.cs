#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Mapify.Editor.Utils;
using UnityEngine;

namespace Mapify.Editor.Validators
{
    public class LightValidator : Validator
    {
        protected override IEnumerator<Result> Validate(Scenes scenes)
        {
            Light[] lights = scenes.gameContentScene.GetAllComponents<Light>();
            int directionalLightCount = lights.Count(light => light.type == LightType.Directional);
            if (directionalLightCount != 1)
                yield return Result.Error($"There must be exactly one directional light in the {scenes.gameContentScene.name} scene. Found {directionalLightCount}");
        }
    }
}
#endif
