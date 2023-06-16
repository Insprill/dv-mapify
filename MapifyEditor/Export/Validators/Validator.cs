#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mapify.Editor.Utils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mapify.Editor.Validators
{
    public abstract class Validator
    {
        private static Dictionary<Scene, bool> sceneStates;
        private static Validator[] validators;

        protected abstract IEnumerator<Result> Validate(Scenes scenes);

        public static IEnumerator<Result> Validate()
        {
            if (validators == null)
                validators = FindValidators();

            (Dictionary<Scene, bool> states, List<string> missingScenes) = Scenes.LoadAllScenes();
            sceneStates = states;

            foreach (string missingScenePath in missingScenes)
                yield return Result.Error($"Failed to find the {missingScenePath.PrettySceneName()} scene! It should be located at \"{missingScenePath}\"");

            if (missingScenes.Count != 0)
                yield break;

            Scenes scenes = Scenes.FromEnumerable(sceneStates.Keys.ToList());
            for (int i = 0; i < validators.Length; i++)
            {
                EditorUtility.DisplayProgressBar("Validating map", null, i / (float)validators.Length);

                Validator validator = validators[i];
                IEnumerator<Result> validation = validator.Validate(scenes);
                while (validation.MoveNext())
                {
                    Result curr = validation.Current;
                    if (curr != null)
                        yield return curr;
                }
            }

            foreach (KeyValuePair<Scene, bool> data in sceneStates)
                if (!data.Value)
                    EditorSceneManager.UnloadSceneAsync(data.Key);

            EditorUtility.ClearProgressBar();
        }

        private static Validator[] FindValidators()
        {
            EditorUtility.DisplayProgressBar("Validating map", "Searching for validators", 0);
            Validator[] arr = Assembly.GetAssembly(typeof(Validator))
                .GetTypes()
                .Where(t => t.IsSubclassOf(typeof(Validator)) && t != typeof(Validator))
                .Select(t =>
                {
                    ConstructorInfo constructor = t.GetConstructor(Type.EmptyTypes);
                    if (constructor != null)
                        return (Validator)constructor.Invoke(null);
                    Debug.LogError($"Could not find a parameterless constructor for type {t.FullName}");
                    return null;
                })
                .Where(v => v != null)
                .ToArray();
            EditorUtility.ClearProgressBar();
            return arr;
        }
    }
}
#endif
