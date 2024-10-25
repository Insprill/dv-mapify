using System.Collections.Generic;
using System.Linq;
using Mapify.Editor.Utils;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace Mapify.Editor
{
    public class Scenes
    {
        public const string TERRAIN = "Assets/Scenes/Terrain.unity";
        public const string RAILWAY = "Assets/Scenes/Railway.unity";
        public const string GAME_CONTENT = "Assets/Scenes/GameContent.unity";
        public const string STREAMING = "Assets/Scenes/Streaming.unity";
        public const string STREAMING_DIR = "Assets/Scenes/Streaming";

        private static readonly string[] ALL_SCENE_PATHS = {
            TERRAIN,
            RAILWAY,
            GAME_CONTENT,
            STREAMING
        };

        public readonly Scene terrainScene;
        public readonly Scene railwayScene;
        public readonly Scene gameContentScene;
        public readonly Scene streamingScene;

        public Scenes(Scene terrainScene, Scene railwayScene, Scene gameContentScene, Scene streamingScene)
        {
            this.terrainScene = terrainScene;
            this.railwayScene = railwayScene;
            this.gameContentScene = gameContentScene;
            this.streamingScene = streamingScene;
        }

        public IEnumerable<Scene> AllScenes()
        {
            return new[] { terrainScene, railwayScene, gameContentScene, streamingScene };
        }

        public static Scenes FromEnumerable(IEnumerable<Scene> scenes)
        {
            IEnumerable<Scene> arr = scenes as Scene[] ?? scenes.ToArray();
            Scene terrainScene = arr.FirstOrDefault(s => s.path == TERRAIN);
            Scene railwayScene = arr.FirstOrDefault(s => s.path == RAILWAY);
            Scene gameContentScene = arr.FirstOrDefault(s => s.path == GAME_CONTENT);
            Scene streamingScene = arr.FirstOrDefault(s => s.path == STREAMING);
            return new Scenes(terrainScene, railwayScene, gameContentScene, streamingScene);
        }

        public static (Dictionary<Scene, bool>, List<string>) LoadAllScenes()
        {
            List<string> missingScenes = new List<string>(0);
            Dictionary<Scene, bool> sceneStates = new Dictionary<Scene, bool>(ALL_SCENE_PATHS.Length);

#if UNITY_EDITOR
            for (int i = 0; i < ALL_SCENE_PATHS.Length; i++)
            {
                string scenePath = ALL_SCENE_PATHS[i];
                EditorUtility.DisplayProgressBar("Loading Scenes", scenePath.PrettySceneName(), i / (float)ALL_SCENE_PATHS.Length);
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) == null)
                {
                    missingScenes.Add(scenePath);
                    continue;
                }

                Scene scene = EditorSceneManager.GetSceneByPath(scenePath);
                bool isSceneLoaded = scene.isLoaded;
                if (!isSceneLoaded)
                    scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                sceneStates.Add(scene, isSceneLoaded);
            }

            EditorUtility.ClearProgressBar();
#endif

            return (sceneStates, missingScenes);
        }
    }
}
