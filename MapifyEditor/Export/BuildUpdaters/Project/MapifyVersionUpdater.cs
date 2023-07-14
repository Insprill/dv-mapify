#if UNITY_EDITOR
using System.IO;
using Mapify.Editor.Utils;

namespace Mapify.Editor.StateUpdaters.Project
{
    public class MapifyVersionUpdater : BuildUpdater
    {
        protected override void Update(Scenes scenes)
        {
            MapInfo mapInfo = EditorAssets.FindAsset<MapInfo>();
            string line = new StreamReader(Names.MAPIFY_VERSION_FILE).ReadLine();
            mapInfo.mapifyVersion = line?.Trim();
        }
    }
}
#endif
