using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DV.PointSet;
using HarmonyLib;
using UnityEngine;

namespace Mapify.Utils
{
    public static class RailwayMeshUpdater
    {
        private static bool cachedValues;
        private static TrackChunkSpatialHash spatialHash;
        private static Dictionary<Vector2Int, List<TrackChunk>> activeChunks;
        private static Dictionary<Vector2Int, List<TrackChunk>> lookupCells;

        //BUG ballast and track meshes aren't generated correctly

        /// <summary>
        ///     Updates the meshes of a RailTrack. Won't do anything if <see cref="RailTrack.generateMeshes" /> is false.
        /// </summary>
        /// <param name="track"></param>
        public static void UpdateTrack(RailTrack track)
        {
            if (!track.generateMeshes || !CheckCaches())
                return;

            RecreateTrack(track);
            UpdateSpatialHash(track);

            RailwayMeshGenerator.Instance.prevCellId = new Vector2Int(int.MinValue, int.MaxValue);
        }

        private static void RecreateTrack(RailTrack track)
        {
            foreach (KeyValuePair<Vector2Int, List<TrackChunk>> kvp in activeChunks)
            {
                var chunks = kvp.Value;
                foreach (var chunk in chunks.Where(chunk => !chunk.isSleepers && chunk.track == track))
                {
                    chunk.ReleasePoolObjects();
                    RailwayMeshGenerator.Instance.ScheduleGenerateBaseAndRail(chunk);
                }
            }
        }

        // This function could use some comments, I really don't understand what it does.
        // - Tostiman
        private static void UpdateSpatialHash(RailTrack track)
        {
            spatialHash.doneAdding = false;

            foreach (var kvp in lookupCells)
                for (int i = kvp.Value.Count - 1; i >= 0; i--)
                    if (kvp.Value[i].track == track)
                        kvp.Value.RemoveAt(i);

            EquiPointSet railPointSet = track.GetPointSet();
            foreach (EquiPointSet.Point point in railPointSet.points)
            {
                TrackChunk trackChunk = spatialHash.Add(railPointSet, point);
                trackChunk.isSleepers = false;
                trackChunk.track = track;

                if (activeChunks.TryGetValue(trackChunk.coords, out List<TrackChunk> chunks) && chunks.All(chunk => chunk.track != track))
                    chunks.Add(trackChunk);
            }

            EquiPointSet sleepersPointSet = EquiPointSet.ResampleEquidistant(railPointSet, track.baseType.sleeperDistance, track.baseType.sleeperDistance * 0.5f, true);
            foreach (EquiPointSet.Point point in sleepersPointSet.points)
            {
                TrackChunk trackChunk = spatialHash.Add(sleepersPointSet, point);
                trackChunk.isSleepers = true;
                trackChunk.track = track;
            }

            spatialHash.DoneAdding();
        }

        private static bool CheckCaches()
        {
            if (cachedValues)
                return true;
            if (RailwayMeshGenerator.Instance == null)
                return false;
            if ((spatialHash = RailwayMeshGenerator.Instance.spatialHash) == null)
                return false;
            if ((activeChunks = RailwayMeshGenerator.Instance.activeChunks) == null)
                return false;
            if ((lookupCells = spatialHash.lookupCells) == null)
                return false;

            cachedValues = true;
            return true;
        }
    }
}
