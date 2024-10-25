#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mapify.Editor.Utils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Mapify.Editor
{
    public static class SceneSplitter
    {
        private static bool isWaitingForCleanup;
        private static string saveDir;
        private static List<Scene> chunkScenes;

        public static SceneSplitData SplitScene(Scene scene, string saveDir, MapInfo mapInfo)
        {
            if (isWaitingForCleanup) throw new InvalidOperationException($"{nameof(SceneSplitter)}#{nameof(Cleanup)} must be called before {nameof(SceneSplitter)}#{nameof(SplitScene)} can be called again!");
            isWaitingForCleanup = true;
            SceneSplitter.saveDir = saveDir;

            EditorUtility.DisplayProgressBar("Mapify", "Splitting scene", 0.0f);

            Dictionary<GameObject, Renderer[]> renderers = scene.GetRootGameObjects()
                .SelectMany(obj =>
                    obj.GetFirstComponentInChildren<LODGroup>()
                        .Cast<Component>()
                        .Concat(obj.GetFirstComponentInChildren<Renderer>())
                        .Select(r => r.gameObject)
                        .Distinct()
                )
                .ToDictionary(obj => obj, obj => obj.GetComponentsInChildren<Renderer>());

            if (renderers.Count == 0)
            {
                EditorUtility.ClearProgressBar();
                return new SceneSplitData();
            }

            (float minX, float minZ, float maxX, float maxZ) = renderers.Values.SelectMany(r => r).GroupedBounds();

            int chunkSize = mapInfo.chunkSize;

            float sceneSizeX = Mathf.CeilToInt(maxX - minX);
            float sceneSizeZ = Mathf.CeilToInt(maxZ - minZ);
            int numChunksX = Mathf.CeilToInt(sceneSizeX / chunkSize);
            int numChunksZ = Mathf.CeilToInt(sceneSizeZ / chunkSize);

            if (Directory.Exists(saveDir))
                Directory.Delete(saveDir, true);
            Directory.CreateDirectory(saveDir);

            int chunkCount = numChunksX * numChunksZ;
            chunkScenes = new List<Scene>(chunkCount);
            List<string> sceneNames = new List<string>(chunkCount);

            for (int chunkX = 0; chunkX < numChunksX; chunkX++)
            for (int chunkZ = 0; chunkZ < numChunksZ; chunkZ++)
            {
                string sceneName = $"{scene.name}__x{chunkX}_z{chunkZ}";
                string scenePath = $"{saveDir}/{sceneName}.unity";
                sceneNames.Add(sceneName);

                // This is *super* sketchy, but Unity won't let me create a new scene in additive mode so this is what we gotta do ¯\_(ツ)_/¯
                using (StreamWriter writer = File.CreateText(scenePath))
                {
                    writer.WriteLine("%YAML 1.1");
                    writer.WriteLine("%TAG !u! tag:unity3d.com,2011:");
                }

                AssetDatabase.Refresh();

                Scene chunkScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                chunkScenes.Add(chunkScene);

                GameObject chunkRoot = new GameObject(sceneName);
                chunkRoot.AddComponent<StreamedObjectInit>().sceneName = sceneName;
                SceneManager.MoveGameObjectToScene(chunkRoot, chunkScene);

                foreach (KeyValuePair<GameObject, Renderer[]> parent in renderers)
                {
                    (float objMinX, float objMinZ, float objMaxX, float objMaxZ) = parent.Value.GroupedBounds();

                    // Check if the object is within the chunk
                    if (objMinX >= minX + chunkX * chunkSize && objMaxX <= minX + (chunkX + 1) * chunkSize &&
                        objMinZ >= minZ + chunkZ * chunkSize && objMaxZ <= minZ + (chunkZ + 1) * chunkSize)
                        Object.Instantiate(parent.Key, chunkRoot.transform, true);
                }

                EditorSceneManager.SaveScene(chunkScene);

                EditorUtility.DisplayProgressBar("Mapify", $"Splitting scene ({chunkX}, {chunkZ})", sceneNames.Count / (float)chunkCount);
            }

            EditorUtility.ClearProgressBar();

            return new SceneSplitData {
                names = sceneNames.ToArray(),
                xSize = chunkSize,
                ySize = 0, // We don't support vertical chunks
                zSize = chunkSize
            };
        }

        public static void Cleanup(bool deleteScenes = true)
        {
            if (chunkScenes != null)
            {
                foreach (Scene scene in chunkScenes)
                    EditorSceneManager.CloseScene(scene, true);
                chunkScenes = null;
            }

            if (deleteScenes && Directory.Exists(saveDir))
            {
                Directory.Delete(saveDir, true);
                File.Delete($"{saveDir}.meta");
            }

            isWaitingForCleanup = false;
        }

        [MenuItem("Mapify/Debug/Split Scene", priority = int.MaxValue)]
        private static void DebugSplitScenes()
        {
            SplitScene(SceneManager.GetSceneByPath(Scenes.STREAMING), Scenes.STREAMING_DIR, EditorAssets.FindAsset<MapInfo>());
            Cleanup(false);
        }
    }
}
#endif
