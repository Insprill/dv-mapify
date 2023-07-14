using System;

namespace Mapify.Editor
{
    [Serializable]
    public struct UnityModManagerInfo
    {
        public string Id;
        public string Version;
        public string DisplayName;
        public string[] LoadAfter;
        public string ManagerVersion;
        public string HomePage;
    }
}
