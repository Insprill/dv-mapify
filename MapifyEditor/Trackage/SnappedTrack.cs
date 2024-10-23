namespace Mapify.Editor
{
    public class SnappedTrack
    {
        public SnappedTrack(Track aTrack, BezierPoint aPoint)
        {
            track = aTrack;
            point = aPoint;
        }
        private Track track;
        private BezierPoint point;

        public void UnSnapped()
        {
            if (track == null){ return; }
            track.UnSnapped(point);
        }
    }
}
