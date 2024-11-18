using System.Collections.Generic;
using System.Linq;
using Priority_Queue;
// using UnityAsync;

// Source, by WallyCZ:
// https://github.com/WallyCZ/DVRouteManager/blob/master/DVRouteManager/PathFinder.cs

namespace Mapify.Utils
{
    public static class PathFinder
    {
        // Heuristic that computes approximate distance between two rails
        private static float Heuristic(RailTrack a, RailTrack b)
        {
            return (a.transform.position - b.transform.position).sqrMagnitude; //we don't need exact distance because that result is used only as a priority
        }

        public struct JunctionSetting
        {
            public Junction junction;
            public int outBranchNr;
        }

        private class RailTrackNode : GenericPriorityQueueNode<double>
        {
            public RailTrack track;

            public RailTrackNode(RailTrack track)
            {
                this.track = track;
            }
        }

        // Return a List of Locations representing the found path
        public static List<RailTrack> FindPath(RailTrack start, RailTrack goal)
        {
            var path = new List<RailTrack>();
            var cameFrom = Astar(start, goal);

            var current = goal;

            // travel backwards through the path
            while (!current.Equals(start))
            {
                if (!cameFrom.ContainsKey(current))
                {
                    Mapify.LogError($"cameFrom does not contain current {current.logicTrack.ID.FullID}");
                    return null;
                }

                path.Add(current);
                current = cameFrom[current];
            }

            if (path.Count > 0)
            {
                path.Add(start);
            }

            return path;
        }

        /// <summary>
        /// A* search
        /// </summary>
        private static Dictionary<RailTrack, RailTrack> Astar(RailTrack start, RailTrack goal)
        {
            var cameFrom = new Dictionary<RailTrack, RailTrack>();
            var costSoFar = new Dictionary<RailTrack, double>();

            var queue = new GenericPriorityQueue<RailTrackNode, double>(10000);
            queue.Enqueue(new RailTrackNode(start), 0.0);

            cameFrom.Add(start, start);
            costSoFar.Add(start, 0.0);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue().track;

                cameFrom.TryGetValue(current, out var prev);

                var debug = $"ID: {current.logicTrack.ID.FullID} Prev: {prev?.logicTrack.ID.FullID}";

                var neighbors = new List<RailTrack>();

                if (current.outIsConnected)
                {
                    neighbors.AddRange(current.GetAllOutBranches().Select(b => b.track));
                }

                if (current.inIsConnected)
                {
                    neighbors.AddRange(current.GetAllInBranches().Select(b => b.track));
                }

                // var branches = DumpNodes(neighbors, current);
                // debug += "\n" + $"all branches: {branches}";

                Mapify.LogDebugExtreme(debug);

                foreach (var neighbor in neighbors)
                {
                    //if we could go through junction directly (without reversing)
                    if (!current.CanGoToDirectly(prev, neighbor))
                    {
                        Mapify.LogDebugExtreme($"{neighbor.logicTrack.ID.FullID} reverse needed");
                        continue;
                    }

                    // compute exact cost
                    var newCost = costSoFar[current] + neighbor.logicTrack.length;

// If there's no cost assigned to the neighbor yet, or if the new
// cost is lower than the assigned one, add newCost for this neighbor
                    if (costSoFar.ContainsKey(neighbor) && !(newCost < costSoFar[neighbor])) continue;

                    // If we're replacing the previous cost, remove it
                    if (costSoFar.ContainsKey(neighbor))
                    {
                        costSoFar.Remove(neighbor);
                        cameFrom.Remove(neighbor);
                    }

                    Mapify.LogDebugExtreme($"neighbor {neighbor.logicTrack.ID.FullID} update {newCost}");

                    costSoFar.Add(neighbor, newCost);
                    cameFrom.Add(neighbor, current);
                    var priority = newCost + Heuristic(neighbor, goal)
                        / 20.0f; //convert distance to time (t = s / v)
                    queue.Enqueue(new RailTrackNode(neighbor), priority);
                }
            }

            return cameFrom;
        }
    }
}
