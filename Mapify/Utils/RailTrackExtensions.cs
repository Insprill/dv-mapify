using System.Linq;

namespace Mapify.Utils
{
    // Source, by WallyCZ:
    // https://github.com/WallyCZ/DVRouteManager/blob/master/DVRouteManager/PathFinder.cs

    public static class RailTrackExtensions
    {
        /// <summary>
        /// If we can go through junction without reversing
        /// </summary>
        private static bool CanGoThroughJunctionDirectly(this RailTrack current, Junction junction, RailTrack from, RailTrack to)
        {
            var fromIsOutBranch = junction != null && junction.outBranches.Any(b => b.track == from);

            if (fromIsOutBranch)
            {
                return false;
            }

            var currentIsOutBranch = junction != null && junction.outBranches.Any(b => b.track == current);

            if (currentIsOutBranch)
            {
                return junction.inBranch.track == to;
            }

            return true;
        }

        public static bool CanGoToDirectly(this RailTrack current, RailTrack from, RailTrack to)
        {
            Junction reversingJunction;
            return CanGoToDirectly(current, from, to, out reversingJunction);
        }

        public static bool CanGoToDirectly(this RailTrack current, RailTrack from, RailTrack to, out Junction reversingJunction)
        {
            reversingJunction = null;

            var isInJuction = current.inIsConnected && current.GetAllInBranches().Any(b => b.track == to);
            var isOutJuction = current.outIsConnected && current.GetAllOutBranches().Any(b => b.track == to);

            if (current.inIsConnected)
            {
                if (isInJuction && CanGoThroughJunctionDirectly(current, current.inJunction, from, to))
                    return true;
            }

            if (current.outIsConnected)
            {
                if (isOutJuction && CanGoThroughJunctionDirectly(current, current.outJunction, from, to))
                    return true;

            }

            if (isInJuction)
            {
                reversingJunction = current.inJunction;
            }

            if (isOutJuction)
            {
                reversingJunction = current.outJunction;
            }

            return false;
        }
    }
}
