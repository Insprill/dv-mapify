using System.Linq;
using Mapify.Utils;
using UnityEngine;

namespace Mapify.CarLabeler
{
    public class YardDestinationComponent : MonoBehaviour
    {
        public YardDestination d;

        public void Setup(YardDestination other)
        {
            d = new YardDestination
            {
                StationID = other.StationID,
                YardID = other.YardID,
                TrackNumber = other.TrackNumber
            };
        }
    }

    public class YardDestination
    {
        // identifies a station
        public string StationID;
        // identifies a yard in a station
        public string YardID;
        // identifies a track in a yard in a station
        public int TrackNumber;

        public string StationName()
        {
            var stationController = StationController.GetStationByYardID(StationID);
            return stationController == null ? "NULL" : stationController.stationInfo.Name;
        }

        /// <summary>
        /// if any of the fields are invalid, set them to something valid
        /// </summary>
        public static void Validate(ref YardDestination aDestination)
        {
            if (aDestination == null)
            {
                aDestination = new YardDestination();
            }

            //TODO stations/yards without tracks?
            if (StationController.GetStationByYardID(aDestination.StationID) == null)
            {
                aDestination.StationID = StationController.allStations.Select(station => station.stationInfo.YardID).FirstOrDefault();
            }

            var stationYardIDs = RailTrackRegistry.Instance.GetSubYardIDsOfYard(aDestination.StationID).ToList();
            if (!stationYardIDs.Contains(aDestination.YardID))
            {
                aDestination.YardID = stationYardIDs.FirstOrDefault();
            }

            var yardTrackNumbers = RailTrackRegistry.Instance.GetTrackNumbersOfSubYard(aDestination.StationID, aDestination.YardID).ToList();
            if (!yardTrackNumbers.Contains(aDestination.TrackNumber))
            {
                aDestination.TrackNumber = yardTrackNumbers.FirstOrDefault();
            }
        }
    }
}
