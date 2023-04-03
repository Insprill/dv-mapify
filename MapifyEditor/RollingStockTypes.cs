using System;
using System.Collections.Generic;

namespace Mapify.Editor
{
    [Serializable]
    // Unity doing Unity things and not supporting nested lists in the editor
    public class RollingStockTypes
    {
        public List<RollingStockType> rollingStockTypes;
    }
}
