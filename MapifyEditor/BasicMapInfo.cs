using System;

namespace Mapify.Editor
{
    [Serializable]
    public class BasicMapInfo
    {
        public string mapName;
        public string version;

        public BasicMapInfo(string mapName, string version)
        {
            this.mapName = mapName;
            this.version = version;
        }

        public bool IsDefault()
        {
            return mapName == Names.DEFAULT_MAP_NAME;
        }
    }
}
