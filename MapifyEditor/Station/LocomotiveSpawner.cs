using System.Collections.Generic;
using UnityEngine;

namespace Mapify.Editor
{
    [RequireComponent(typeof(Track))]
    public class LocomotiveSpawner : MonoBehaviour
    {
        // Updated in the inspector
#pragma warning disable CS0649
        [SerializeField]
        internal List<RollingStockTypes> locomotiveTypesToSpawn;
#pragma warning restore CS0649
        [HideInInspector] // You can't edit the property drawer of collections themselves :|
        public List<string> condensedLocomotiveTypes; // Workaround for Unity being stupid as always
        public Track Track => GetComponent<Track>();
    }
}
