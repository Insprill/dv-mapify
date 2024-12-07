using CommsRadioAPI;

namespace Mapify.CarLabeler
{
    public class SelectStation: SubState
    {
        public SelectStation(YardDestination aDestination) :
            base(aDestination, $"station:\n{aDestination.StationName()}"
        )
        {}

        public override AStateBehaviour OnAction(CommsRadioUtility utility, InputAction action)
        {
            switch (action)
            {
                case InputAction.Activate:
                    utility.PlaySound(VanillaSoundCommsRadio.Confirm);
                    return new SelectYard(destination);
                case InputAction.Up:
                    utility.PlaySound(VanillaSoundCommsRadio.Switch);
                    return NextOrPreviousStation(1);
                case InputAction.Down:
                    utility.PlaySound(VanillaSoundCommsRadio.Switch);
                    return NextOrPreviousStation(-1);
                default:
                    return new SelectStation(destination);
            }
        }

        private AStateBehaviour NextOrPreviousStation(int stationIndexDelta)
        {
            var allStations = StationController.allStations;
            var stationIndex = 0;

            for (int index = 0; index < allStations.Count; index++)
            {
                if (allStations[index].stationInfo.YardID != destination.StationID) continue;

                stationIndex = index;
                break;
            }

            stationIndex = Utils.Misc.BetterModulo(stationIndex + stationIndexDelta, allStations.Count);

            destination.StationID = allStations[stationIndex].stationInfo.YardID;

            return new SelectStation(destination);
        }
    }
}
