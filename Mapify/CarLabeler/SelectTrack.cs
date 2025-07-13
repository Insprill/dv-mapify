using System.Linq;
using CommsRadioAPI;
using Mapify.Utils;

namespace Mapify.CarLabeler
{
    public class SelectTrack: SubState
    {
        public SelectTrack(YardDestination aDestination) :
            base(aDestination, $"track: {aDestination.TrackNumber}"
        )
        {}

        public override AStateBehaviour OnAction(CommsRadioUtility utility, InputAction action)
        {
            switch (action)
            {
                case InputAction.Activate:
                    utility.PlaySound(VanillaSoundCommsRadio.Confirm);
                    return new CreateLabel(destination);
                case InputAction.Up:
                    utility.PlaySound(VanillaSoundCommsRadio.Switch);
                    return NextOrPreviousTrack(1);
                case InputAction.Down:
                    utility.PlaySound(VanillaSoundCommsRadio.Switch);
                    return NextOrPreviousTrack(-1);
                default:
                    return new SelectTrack(destination);
            }
        }

        private AStateBehaviour NextOrPreviousTrack(int trackIndexDelta)
        {
            //TODO cache this
            var yardTracks = RailTrackRegistry.Instance
                .GetTrackNumbersOfSubYard(destination.StationID, destination.YardID)
                .ToList();

            Mapify.LogDebug("yardTracks:");
            foreach (var track in yardTracks)
            {
                Mapify.LogDebug(track);
            }

            var trackIndex = 0;

            for (int index = 0; index < yardTracks.Count; index++)
            {
                if (yardTracks[index] != destination.TrackNumber) continue;

                trackIndex = index;
                break;
            }

            trackIndex = Utils.Misc.BetterModulo(trackIndex + trackIndexDelta, yardTracks.Count);
            destination.TrackNumber = yardTracks[trackIndex];

            return new SelectTrack(destination);
        }
    }
}
