using System;

namespace Mapify.Editor
{
    [Serializable]
    public class SceneSplitData
    {
        public string[] names;
        public int xSize;
        public int ySize;
        public int zSize;

        // Not sure *exactly* what these do but this is what DV sets them to ¯\_(ツ)_/¯
        public int xLimitsy = 15;
        public int zLimitsy = 15;
    }
}
