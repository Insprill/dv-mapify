using System.Collections.Generic;
using UnityEngine;

namespace Mapify.Editor
{
    public class Station : MonoBehaviour
    {
        [Header("Station Info")]
        public string displayName;
        public string yardID;
        public Color color;
        public Transform teleportLocation;

        [Header("Station Tracks")]
        public List<string> storageTrackNames;
        public List<string> transferInTrackNames;
        public List<string> transferOutTrackNames;

        [Header("Jobs")]
        [Tooltip("The area where job booklets should spawn")]
        public BoxCollider bookletSpawnArea;
        public int jobsCapacity = 30;
        public int maxShuntingStorageTracks = 3;
        public int minCarsPerJob = 3;
        public int maxCarsPerJob = 20;
        [Header("Cargo groups")]
        // public List<CargoGroup> inputCargoGroups;
        // public List<CargoGroup> outputCargoGroups;
        [Header("Starting chain job priorities")]
        public bool loadStartingJobSupported = true;
        public bool haulStartingJobSupported = true;
        public bool unloadStartingJobSupported = true;
        public bool emptyHaulStartingJobSupported = true;
    }
}
