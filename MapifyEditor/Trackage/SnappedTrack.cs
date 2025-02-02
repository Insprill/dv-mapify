namespace Mapify.Editor
{
    public class SnappedTrack
    {
        private Track track;
        private BezierPoint point;

        public SnappedTrack(Track aTrack, BezierPoint aPoint)
        {
            track = aTrack;
            point = aPoint;
        }

        public void UnSnapped()
        {
            if (track == null){ return; }
            track.UnSnapped(point);
        }
    }
}
