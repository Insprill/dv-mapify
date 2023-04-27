using System.Collections.Generic;
using System.Reflection;
using DV.PointSet;
using HarmonyLib;
using UnityEngine;

namespace Mapify.Utils
{
    public static class RailwayMeshUpdater
    {
        private static readonly FieldInfo RailwayMeshGenerator_Field_spatialHash = AccessTools.DeclaredField(typeof(RailwayMeshGenerator), "spatialHash");
        private static readonly FieldInfo RailwayMeshGenerator_Field_activeChunks = AccessTools.DeclaredField(typeof(RailwayMeshGenerator), "activeChunks");
        private static readonly FieldInfo RailwayMeshGenerator_Field_prevCellId = AccessTools.DeclaredField(typeof(RailwayMeshGenerator), "prevCellId");
        private static readonly FieldInfo TrackChunkSpatialHash_Field_doneAdding = AccessTools.DeclaredField(typeof(TrackChunkSpatialHash), "doneAdding");
        private static readonly FieldInfo TrackChunkSpatialHash_Field_lookupCells = AccessTools.DeclaredField(typeof(TrackChunkSpatialHash), "lookupCells");

        private static bool cachedValues;
        private static RailwayMeshGenerator railwayMeshGenerator;
        private static TrackChunkSpatialHash spatialHash;
        private static Dictionary<Vector2Int, List<TrackChunk>> activeChunks;
        private static Dictionary<Vector2Int, List<TrackChunk>> lookupCells;

        public static void UpdateTrack(RailTrack track)
        {
            if (!CheckCaches())
                return;

            TrackChunkSpatialHash_Field_doneAdding.SetValue(spatialHash, false);

            foreach (KeyValuePair<Vector2Int, List<TrackChunk>> kvp in lookupCells)
                for (int i = kvp.Value.Count - 1; i >= 0; i--)
                    if (kvp.Value[i].track == track)
                        kvp.Value.RemoveAt(i);

            EquiPointSet railPointSet = track.GetPointSet();
            foreach (EquiPointSet.Point point in railPointSet.points)
            {
                TrackChunk trackChunk = spatialHash.Add(railPointSet, point);
                trackChunk.isSleepers = false;
                trackChunk.track = track;
            }

            EquiPointSet sleepersPointSet = EquiPointSet.ResampleEquidistant(railPointSet, track.baseType.sleeperDistance, track.baseType.sleeperDistance * 0.5f, true);
            foreach (EquiPointSet.Point point in sleepersPointSet.points)
            {
                TrackChunk trackChunk = spatialHash.Add(sleepersPointSet, point);
                trackChunk.isSleepers = true;
                trackChunk.track = track;
            }

            spatialHash.DoneAdding();
            RailwayMeshGenerator_Field_prevCellId.SetValue(railwayMeshGenerator, new Vector2Int(int.MinValue, int.MaxValue));
            foreach (KeyValuePair<Vector2Int, List<TrackChunk>> kvp in activeChunks)
            foreach (TrackChunk chunk in kvp.Value)
                if (chunk.track == track)
                    chunk.ReleasePoolObjects();
            // activeChunks.Clear();
        }

        private static bool CheckCaches()
        {
            if (cachedValues)
                return true;
            railwayMeshGenerator = Object.FindObjectOfType<RailwayMeshGenerator>();
            if (railwayMeshGenerator == null)
                return false;
            spatialHash = (TrackChunkSpatialHash)RailwayMeshGenerator_Field_spatialHash.GetValue(railwayMeshGenerator);
            activeChunks = (Dictionary<Vector2Int, List<TrackChunk>>)RailwayMeshGenerator_Field_activeChunks.GetValue(railwayMeshGenerator);
            lookupCells = (Dictionary<Vector2Int, List<TrackChunk>>)TrackChunkSpatialHash_Field_lookupCells.GetValue(spatialHash);
            cachedValues = true;
            return true;
        }
    }
}
