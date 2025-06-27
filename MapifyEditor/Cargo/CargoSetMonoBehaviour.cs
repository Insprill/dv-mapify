using System.Collections.Generic;
using UnityEngine;

namespace Mapify.Editor
{
    // I can't with Unity man...
    [ExecuteInEditMode]
    public class CargoSetMonoBehaviour : MonoBehaviour
    {
        public List<Cargo> cargoTypes;
        public List<Station> stations;

        public CargoSet ToOriginal()
        {
            return new CargoSet {
                cargoTypes = cargoTypes,
                stations = stations
            };
        }

        private void OnEnable()
        {
            //hide this script in the editor
            hideFlags = HideFlags.HideInInspector;
        }
    }
}
