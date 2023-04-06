using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mapify.Editor
{
    [Serializable]
    public class CargoSet
    {
        public List<Cargo> cargoTypes;
        public List<Station> stations;

        public CargoSetMonoBehaviour ToMonoBehaviour(GameObject gameObject)
        {
            CargoSetMonoBehaviour mb = gameObject.AddComponent<CargoSetMonoBehaviour>();
            mb.cargoTypes = cargoTypes;
            mb.stations = stations;
            return mb;
        }
    }
}
