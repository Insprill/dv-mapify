using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Mapify.Editor
{
    public class Station : MonoBehaviour
    {
        [Header("Station Info")]
        [Tooltip("The display name of the station")]
        public string stationName;
        [Tooltip("The 2-3 character ID of the station (e.g. HB for Harbor, SM for Steel Mill, etc)")]
        public string stationID;
        [Tooltip("The color of the station shown on job booklets")]
        public Color color;
        [Tooltip("The location where the player will be teleported to when fast travelling")]
        public Transform teleportLocation;

        [HideInInspector]
        public List<string> storageTrackNames;
        [HideInInspector]
        public List<string> transferInTrackNames;
        [HideInInspector]
        public List<string> transferOutTrackNames;

        [Header("Jobs")]
        [Tooltip("The area where job booklets should spawn. Not required when using a vanilla station")]
        public BoxCollider bookletSpawnArea;
        [Tooltip("The rough center of the yard. Used at the reference point for generating jobs. Will use the station if unset")]
        public Transform yardCenter;
        [Tooltip("The distance, in meters, the player has to be relative to the station for job overview booklets to generate")]
        public float bookletGenerationDistance = 150;
        [Tooltip("The distance, in meters, the player has to be relative to the yard center for jobs to generate")]
        public float jobGenerationDistance = 500;
        [Tooltip("The distance, in meters, the player has to be relative to the yard center for jobs to despawn")]
        public float jobDestroyDistance = 600;
        [Range(1, 30)]
        public int jobsCapacity = 30;
        public int maxShuntingStorageTracks = 3;
        public int minCarsPerJob = 3;
        public int maxCarsPerJob = 20;

        [Header("Cargo")]
        [HideInInspector]
        [SerializeField]
        public List<WarehouseMachine> warehouseMachines;
        // Another workaround for Unity's excuse of a game engine
        [HideInNormalInspector]
        [SerializeField]
        public int inputCargoGroupsCount;
#pragma warning disable CS0649
        [SerializeField]
        internal List<CargoSet> inputCargoGroups;
        [SerializeField]
        internal List<CargoSet> outputCargoGroups;
#pragma warning restore CS0649

        [Header("Starting chain job priorities")]
        public bool loadStartingJobSupported = true;
        public bool haulStartingJobSupported = true;
        public bool unloadStartingJobSupported = true;
        public bool emptyHaulStartingJobSupported = true;
    }
}
