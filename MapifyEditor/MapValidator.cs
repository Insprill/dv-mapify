using System.Collections.Generic;
using System.Linq;
using Mapify.Editor.Validators;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Mapify.Editor
{
    // Me: Can we have CCLs TrainCarValidator?
    // Mom: We have CCLs TrainCarValidator at home
    // TrainCarValidator at home:
    public static class MapValidator
    {
        public static IEnumerator<Result> Validate()
        {
            Validator[] validators = {
                new ProjectValidator(),
                new RailwaySceneValidator(),
                new TerrainSceneValidator(),
                new GameContentSceneValidator()
            };

            Dictionary<Scene, bool> sceneStates = new Dictionary<Scene, bool>(validators.Length);

            foreach (Validator validator in validators)
            {
                if (!(validator is SceneValidator sceneValidator)) continue;
                string scenePath = sceneValidator.GetScenePath();
                Scene scene = EditorSceneManager.GetSceneByPath(scenePath);
                if (!scene.IsValid())
                {
                    yield return Result.Error($"Failed to find {sceneValidator.GetPrettySceneName()} scene! It should be located at \"{scenePath}\"");
                    yield break;
                }

                bool isSceneLoaded = scene.isLoaded;
                if (!isSceneLoaded)
                    EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                sceneStates.Add(scene, isSceneLoaded);
            }

            foreach (Validator validator in validators)
            {
                List<Scene> scenes = sceneStates.Keys.ToList();
                IEnumerator<Result> results = validator.Validate(scenes);
                while (results.MoveNext()) yield return results.Current;
            }

            foreach (KeyValuePair<Scene, bool> data in sceneStates)
                if (!data.Value)
                    EditorSceneManager.UnloadSceneAsync(data.Key);
        }
    }
}
