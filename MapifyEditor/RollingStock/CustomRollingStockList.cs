using System;
using System.Collections.Generic;

namespace Mapify.Editor
{
    [Serializable]
    // Unity doing Unity things and not supporting nested lists in the editor
    // Unity also doing Unity things by only supporting generics in 2020.1 and up
    public class CustomRollingStockList
    {
        public List<string> rollingStock;
    }
}
