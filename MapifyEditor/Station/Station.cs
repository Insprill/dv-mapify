using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

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

        [Header("Job Generation")]
        [Tooltip("All warehouse machines associated with this station")]
        public WarehouseMachine[] warehouseMachines;
        [Tooltip("The area where job booklets should spawn. Not required when using a vanilla station")]
        public BoxCollider bookletSpawnArea;
        [SerializeField]
        [Tooltip("The rough center of the yard. Used at the reference point for generating jobs. Will use the station if unset")]
        private Transform yardCenter;
        public Transform YardCenter => yardCenter != null ? yardCenter : transform;
        [Tooltip("The distance, in meters, the player has to be relative to the station for job overview booklets to generate")]
        public float bookletGenerationDistance = 150;
        [Tooltip("The distance, in meters, the player has to be relative to the yard center for jobs to generate")]
        public float jobGenerationDistance = 500;
        [Tooltip("The distance, in meters, the player has to be relative to the yard center for jobs to despawn")]
        public float jobDestroyDistance = 600;
        [Range(1, 999)]
        [Tooltip("The maximum number of jobs that can be generated at once. This number may not be met, but it'll never be exceeded")]
        public int jobsCapacity = 30;
        [Tooltip("The minimum number of cars per-job")]
        public int minCarsPerJob = 3;
        [Tooltip("The maximum number of cars per-job")]
        public int maxCarsPerJob = 20;
        public int maxShuntingStorageTracks = 3;

        [Tooltip("Whether freight haul jobs will be generated")]
        public bool generateFreightHaul = true;
        [Tooltip("Whether logistical haul jobs will be generated")]
        public bool generateLogisticalHaul = true;
        [Tooltip("Whether shunting load jobs will be generated")]
        public bool generateShuntingLoad = true;
        [Tooltip("Whether shunting unload jobs will be generated")]
        public bool generateShuntingUnload = true;

        [Header("Cargo")]
        [SerializeField]
        internal List<CargoSet> inputCargoGroups;
        [SerializeField]
        internal List<CargoSet> outputCargoGroups;
        // Another workaround for Unity's excuse of a game engine
        [HideInNormalInspector]
        public int inputCargoGroupsCount;
        [HideInInspector]
        public List<string> storageTrackNames;
        [HideInInspector]
        public List<string> transferInTrackNames;
        [HideInInspector]
        public List<string> transferOutTrackNames;

        #region Editor Visualization

#if UNITY_EDITOR

        [Header("Editor Visualization")]
        [SerializeField]
        private bool visualizeJobGenerationRange;
        [SerializeField]
        private bool visualizeBookletGenerationDistance;
        [SerializeField]
        private bool visualizeJobDestroyDistance;

        private void OnDrawGizmos()
        {
            if (visualizeJobDestroyDistance)
            {
                Handles.color = new Color32(200, 25, 25, 100);
                Handles.DrawSolidDisc(YardCenter.position, Vector3.up, jobDestroyDistance);
            }

            if (visualizeJobGenerationRange)
            {
                Handles.color = new Color32(25, 200, 25, 100);
                Handles.DrawSolidDisc(YardCenter.position, Vector3.up, jobGenerationDistance);
            }

            if (visualizeBookletGenerationDistance)
            {
                Handles.color = new Color32(25, 25, 200, 100);
                Handles.DrawSolidDisc(YardCenter.position, Vector3.up, bookletGenerationDistance);
            }
        }

#endif

        #endregion
    }
}
