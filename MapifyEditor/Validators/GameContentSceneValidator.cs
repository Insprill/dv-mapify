using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mapify.Editor.Validators
{
    public class GameContentSceneValidator : SceneValidator
    {
        protected override IEnumerator<Result> ValidateScene(Scene scene)
        {
            GameObject[] roots = scene.GetRootGameObjects();

            Light[] lights = roots.SelectMany(go => go.GetComponentsInChildren<Light>()).ToArray();

            int directionalLightCount = lights.Count(light => light.type == LightType.Directional);
            if (directionalLightCount > 1)
                yield return Result.Error($"There must be exactly one directional light in the {GetPrettySceneName()} scene. Found {directionalLightCount}");
        }

        protected override string GetScenePath()
        {
            return "Assets/Scenes/GameContent.unity";
        }
    }
}
