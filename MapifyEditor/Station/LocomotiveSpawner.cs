using System.Collections.Generic;
using UnityEngine;

namespace Mapify.Editor
{
    [RequireComponent(typeof(Track))]
    public class LocomotiveSpawner : MonoBehaviour
    {
        public List<List<RollingStockType>> locomotiveTypesToSpawn;
        [HideInNormalInspector]
        public Station closestStation;
        public Track Track => GetComponent<Track>();
    }
}
