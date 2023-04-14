using System.Collections.Generic;
using UnityEngine;

namespace Mapify.Editor
{
    public class WarehouseMachine : MonoBehaviour
    {
        [Tooltip("The Station ID of the track this machine is assigned to")]
        public string loadingTrackStationId;
        [Tooltip("The Yard ID of the track this machine is assigned to")]
        public char loadingTrackYardId;
        [Tooltip("The Track ID of the track this machine is assigned to")]
        public byte loadingTrackId;
        [Tooltip("What all cargo types this machine supports")]
        public List<Cargo> supportedCargoTypes;

        public Track LoadingTrack => Track.Find(loadingTrackStationId, loadingTrackYardId, loadingTrackId, TrackType.Loading);
    }
}
