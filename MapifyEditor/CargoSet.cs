using System;
using System.Collections.Generic;
using UnityEditor;

namespace Mapify.Editor
{
    [Serializable]
    public class CargoSet
    {
        public List<Cargo> cargoTypes;
        public List<Station> stations;

        public string Serialize()
        {
            return EditorJsonUtility.ToJson(this);
        }
    }
}
