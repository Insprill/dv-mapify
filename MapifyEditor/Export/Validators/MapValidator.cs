using System.Collections.Generic;
using System.Linq;
using Mapify.Editor.Utils;
using Mapify.Editor.Validators;
using UnityEditor;
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
            Cleanup();
            EditorUtility.DisplayProgressBar("Mapify", "Validating map", 0);

            validators = new Validator[] {
                new ProjectValidator(),
                new RailwaySceneValidator(),
                new TerrainSceneValidator(),
                new GameContentSceneValidator()
            };

            int validatorCount = validators.Length;
            sceneStates = new Dictionary<Scene, bool>(validatorCount);

            gameObjectsToUndo = new List<Object>();
            for (int i = 0; i < validators.Length; i++)
            {
                Validator validator = validators[i];
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
                EditorUtility.DisplayProgressBar("Mapify", "Validating map", i / (validatorCount * 2f));
            }

            List<Result> results = gameObjectsToUndo.RecordObjectChanges(() =>
            {
                List<Result> tmp = new List<Result>();
                for (int i = 0; i < validators.Length; i++)
                {
                    Validator validator = validators[i];
                    List<Scene> scenes = sceneStates.Keys.ToList();
                    IEnumerator<Result> validation = validator.Validate(scenes);
                    while (validation.MoveNext()) tmp.Add(validation.Current);
                    EditorUtility.DisplayProgressBar("Mapify", "Validating map", i / (float)validatorCount);
                }

                return tmp;
            });

            foreach (Result result in results)
                if (result != null)
                    yield return result;

            EditorUtility.ClearProgressBar();
        }

        public static void Cleanup()
        {
            if (gameObjectsToUndo == null)
                return;
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
            gameObjectsToUndo = null;
        }
    }
}
