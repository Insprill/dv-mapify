#if UNITY_EDITOR
using Mapify.Editor.Utils;

namespace Mapify.Editor.StateUpdaters
{
    public class TrackUpdater : BuildUpdater
    {
        protected override void Update(Scenes scenes)
        {
            Track[] tracks = scenes.railwayScene.GetAllComponents<Track>();
            int roadId = 1;
            foreach (Track track in tracks)
            {
                if (track.IsSwitch || track.IsTurntable)
                    continue;

                // Tracks starting with [Y] or [#] don't get signs generated (SignPlacer#ShouldIncludeTrack)
                track.name = track.trackType == TrackType.Road
                    ? $"{(track.generateSigns ? "" : "[#] ")}Road {roadId++}"
                    : $"[Y]_[{track.stationId}]_[{track.yardId}-{track.trackId:D2}-{track.trackType.LetterId()}]";
            }
        }
    }
}
#endif
