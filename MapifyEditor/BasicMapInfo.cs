using System;
using UnityEngine;

namespace Mapify.Editor
{
    [Serializable]
    public struct BasicMapInfo
    {
        public string name;
        public string version;
        public string mapifyVersion;

        [Obsolete("Used before 0.4.0", true)]
        public string mapName {
            set => name = value;
        }

        public BasicMapInfo(string name, string version, string mapifyVersion)
        {
            this.name = name;
            this.version = version;
            this.mapifyVersion = mapifyVersion;
        }

        public bool IsDefault()
        {
            return name == Names.DEFAULT_MAP_NAME;
        }

        public static BasicMapInfo FromMapInfo(MapInfo mapInfo)
        {
            string json = JsonUtility.ToJson(mapInfo);
            return JsonUtility.FromJson<BasicMapInfo>(json);
        }
    }
}
