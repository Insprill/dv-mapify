using System.Collections.Generic;
using UnityEngine;

namespace Mapify.Editor
{
    [RequireComponent(typeof(Track))]
    public class LocomotiveSpawner : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        [Tooltip("What all locomotives to spawn. Each element is a group to spawn together (e.g. the steamer and it's tender)")]
        internal List<RollingStockTypes> locomotiveTypesToSpawn;

        [SerializeField]
        [Tooltip("Whether to flip the orientation of the spawned locomotive(s)")]
        public bool flipOrientation;
#pragma warning restore CS0649

        [HideInInspector] // You can't edit the property drawer of collections themselves :|
        public List<string> condensedLocomotiveTypes; // Workaround for Unity being stupid as always
        public Track Track => GetComponent<Track>();
    }
}
