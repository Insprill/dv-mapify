using System.Linq;
using CommsRadioAPI;
using Mapify.Utils;

namespace Mapify.CarLabeler
{
    public class SelectYard: SubState
    {
        public SelectYard(YardDestination aDestination) :
            base(aDestination, $"yard: {aDestination.YardID}"
        )
        {}

        public override AStateBehaviour OnAction(CommsRadioUtility utility, InputAction action)
        {
            switch (action)
            {
                case InputAction.Activate:
                    utility.PlaySound(VanillaSoundCommsRadio.Confirm);
                    return new SelectTrack(destination);
                case InputAction.Up:
                    utility.PlaySound(VanillaSoundCommsRadio.Switch);
                    return NextOrPreviousYard(1);
                case InputAction.Down:
                    utility.PlaySound(VanillaSoundCommsRadio.Switch);
                    return NextOrPreviousYard(-1);
                default:
                    return new SelectYard(destination);
            }
        }

        private AStateBehaviour NextOrPreviousYard(int yardIndexDelta)
        {
            //TODO cache this
            var stationYards = RailTrackRegistry.Instance.GetSubYardIDsOfYard(destination.StationID).ToList();

            Mapify.LogDebug("stationYards:");
            foreach (var yard in stationYards)
            {
                Mapify.LogDebug(yard);
            }

            var yardIndex = 0;

            for (int index = 0; index < stationYards.Count; index++)
            {
                if (stationYards[index] != destination.YardID) continue;

                yardIndex = index;
                break;
            }

            yardIndex = Utils.Misc.BetterModulo(yardIndex + yardIndexDelta, stationYards.Count);
            destination.YardID = stationYards[yardIndex];

            return new SelectYard(destination);
        }
    }
}
