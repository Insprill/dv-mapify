using System;

namespace Mapify.Editor
{
    [Serializable]
    public struct UnityModManagerInfo
    {
        public string Id;
        public string Version;
        public string DisplayName;
        public string ManagerVersion;
        public string[] Requirements;
        public string HomePage;
        public string Repository;
    }
}
