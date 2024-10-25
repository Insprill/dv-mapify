using Mapify.Editor.Utils;
using UnityEngine;

#if UNITY_EDITOR
namespace Mapify.Editor.StateUpdaters
{
    public class StreamingUpdater : BuildUpdater
    {
        protected override void Update(Scenes scenes)
        {
            MapInfo mapInfo = EditorAssets.FindAsset<MapInfo>();
            SceneSplitData splitData = SceneSplitter.SplitScene(scenes.streamingScene, Scenes.STREAMING_DIR, mapInfo);
            mapInfo.sceneSplitData = JsonUtility.ToJson(splitData);
        }

        protected override void Cleanup(Scenes scenes)
        {
            SceneSplitter.Cleanup();
        }
    }
}
#endif
