using System.Collections.Generic;
using UnityEngine;

namespace Mapify.Editor
{
    public abstract class LocomotiveSpawner : MonoBehaviour
    {
        [SerializeField]
        [HideInInspector]
        [Tooltip("The Station ID of the track this LocomotiveSpawner should spawn on")]
        public string loadingTrackStationId;
        [SerializeField]
        [HideInInspector]
        [Tooltip("The Yard ID of the track this LocomotiveSpawner should spawn on")]
        public char loadingTrackYardId;
        [SerializeField]
        [HideInInspector]
        [Tooltip("The Track ID of the track this LocomotiveSpawner should spawn on")]
        public byte loadingTrackId;
        [SerializeField]
        [Tooltip("Whether to flip the orientation of the spawned locomotive(s)")]
        public bool flipOrientation;

        [HideInInspector] // You can't edit the property drawer of collections themselves :|
        public string[] condensedLocomotiveTypes; // Workaround for Unity being stupid as always

        public Track Track {
            get {
                Track selfTrack = GetComponent<Track>();
                return selfTrack ? selfTrack : Track.Find(loadingTrackStationId, loadingTrackYardId, loadingTrackId, TrackType.Parking);
            }
        }

        public abstract IEnumerable<string> CondenseLocomotiveTypes();
    }
}
