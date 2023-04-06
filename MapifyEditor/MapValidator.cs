using System.Collections.Generic;
using System.Linq;
using Mapify.Editor.Utils;
using Mapify.Editor.Validators;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mapify.Editor
{
    // Me: Can we have CCLs TrainCarValidator?
    // Mom: We have CCLs TrainCarValidator at home
    // TrainCarValidator at home:
    public static class MapValidator
    {
        private static Dictionary<Scene, bool> sceneStates;
        private static Validator[] validators;
        private static List<Object> gameObjectsToUndo;

        public static IEnumerator<Result> Validate()
        {
            validators = new Validator[] {
                new ProjectValidator(),
                new RailwaySceneValidator(),
                new TerrainSceneValidator(),
                new GameContentSceneValidator()
            };

            sceneStates = new Dictionary<Scene, bool>(validators.Length);

            gameObjectsToUndo = new List<Object>();
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

                gameObjectsToUndo.AddRange(scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<Component>(true)));
            }

            List<Result> results = gameObjectsToUndo.RecordObjectChanges(() =>
            {
                List<Result> tmp = new List<Result>();
                foreach (Validator validator in validators)
                {
                    List<Scene> scenes = sceneStates.Keys.ToList();
                    IEnumerator<Result> validation = validator.Validate(scenes);
                    while (validation.MoveNext()) tmp.Add(validation.Current);
                }

                return tmp;
            });

            foreach (Result result in results)
                yield return result;
        }

        public static void Cleanup()
        {
            gameObjectsToUndo.RecordObjectChanges<object>(() =>
            {
                if (validators != null)
                    foreach (Validator validator in validators)
                        validator.Cleanup();

                if (sceneStates != null)
                    foreach (KeyValuePair<Scene, bool> data in sceneStates)
                        if (!data.Value)
                            EditorSceneManager.UnloadSceneAsync(data.Key);
                return null;
            });
        }
    }
}
