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
using Object = UnityEngine.Object;

namespace Mapify.Editor.StateUpdaters
{
    public abstract class BuildUpdater
    {
        private static Dictionary<Scene, bool> sceneStates;
        private static BuildUpdater[] updaters;
        private static List<Object> gameObjectsToUndo;

        protected abstract void Update(Scenes scenes);

        protected virtual void Cleanup(Scenes scenes)
        { }

        [MenuItem("Mapify/Debug/Build Update/Update", priority = int.MaxValue)]
        private static void Update(MenuCommand command)
        {
            Update();
        }

        public static void Update()
        {
            Cleanup();

            if (updaters == null)
                updaters = FindUpdaters();

            sceneStates = Scenes.LoadAllScenes().Item1;

            gameObjectsToUndo = sceneStates.Keys
                .SelectMany(s => s.GetAllComponents<Component>(true))
                .Concat(sceneStates.Keys
                    .SelectMany(s => s.GetAllComponents<Transform>()
                        .Select(t => (Object)t.gameObject)
                    )
                ).ToList();
            gameObjectsToUndo.Add(EditorAssets.FindAsset<MapInfo>());

            Scenes scenes = Scenes.FromEnumerable(sceneStates.Keys);
            gameObjectsToUndo.RecordObjectChanges(() =>
            {
                for (int i = 0; i < updaters.Length; i++)
                {
                    updaters[i].Update(scenes);
                    EditorUtility.DisplayProgressBar("Updating internal fields", null, i / (float)updaters.Length);
                }
            });

            EditorUtility.ClearProgressBar();
        }

        [MenuItem("Mapify/Debug/Build Update/Cleanup", priority = int.MaxValue)]
        private static void Cleanup(MenuCommand command)
        {
            Cleanup();
        }

        public static void Cleanup()
        {
            if (gameObjectsToUndo != null)
            {
                gameObjectsToUndo.RecordObjectChanges(() =>
                {
                    if (updaters == null) return;
                    Scenes scenes = Scenes.FromEnumerable(sceneStates.Keys);
                    foreach (BuildUpdater updater in updaters)
                        updater.Cleanup(scenes);
                });
                gameObjectsToUndo = null;
            }

            if (sceneStates != null)
            {
                foreach (KeyValuePair<Scene, bool> data in sceneStates)
                    if (!data.Value)
                        EditorSceneManager.UnloadSceneAsync(data.Key);
                sceneStates = null;
            }
        }

        private static BuildUpdater[] FindUpdaters()
        {
            EditorUtility.DisplayProgressBar("Validating map", "Searching for validators", 0);
            BuildUpdater[] arr = Assembly.GetAssembly(typeof(BuildUpdater))
                .GetTypes()
                .Where(t => t.IsSubclassOf(typeof(BuildUpdater)) && t != typeof(BuildUpdater))
                .Select(t =>
                {
                    ConstructorInfo constructor = t.GetConstructor(Type.EmptyTypes);
                    if (constructor != null)
                        return (BuildUpdater)constructor.Invoke(null);
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
