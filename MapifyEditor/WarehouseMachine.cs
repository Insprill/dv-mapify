using System.Collections.Generic;
using UnityEngine;

namespace Mapify.Editor
{
    public class WarehouseMachine : MonoBehaviour
    {
        public string loadingTrackStationId;
        public char loadingTrackYardId;
        public byte loadingTrackId;
        public List<Cargo> supportedCargoTypes;

        public Track LoadingTrack => Track.Find(loadingTrackStationId, loadingTrackYardId, loadingTrackId, TrackType.Loading);
    }
}
