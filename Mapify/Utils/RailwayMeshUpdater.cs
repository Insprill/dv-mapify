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
        private static readonly MethodInfo RailwayMeshGenerator_Method_ScheduleGenerateBaseAndRail = AccessTools.DeclaredMethod(typeof(RailwayMeshGenerator), "ScheduleGenerateBaseAndRail");
        private static readonly FieldInfo TrackChunkSpatialHash_Field_doneAdding = AccessTools.DeclaredField(typeof(TrackChunkSpatialHash), "doneAdding");
        private static readonly FieldInfo TrackChunkSpatialHash_Field_lookupCells = AccessTools.DeclaredField(typeof(TrackChunkSpatialHash), "lookupCells");

        private static bool cachedValues;
        private static RailwayMeshGenerator railwayMeshGenerator;
        private static TrackChunkSpatialHash spatialHash;
        private static Dictionary<Vector2Int, List<TrackChunk>> activeChunks;
        private static Dictionary<Vector2Int, List<TrackChunk>> lookupCells;

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

            RailwayMeshGenerator_Field_prevCellId.SetValue(railwayMeshGenerator, new Vector2Int(int.MinValue, int.MaxValue));
        }

        private static void RecreateTrack(RailTrack track)
        {
            foreach (KeyValuePair<Vector2Int, List<TrackChunk>> kvp in activeChunks)
            {
                List<TrackChunk> chunks = kvp.Value;
                foreach (TrackChunk chunk in chunks)
                {
                    if (chunk.isSleepers || chunk.track != track)
                        continue;
                    chunk.ReleasePoolObjects();
                    RailwayMeshGenerator_Method_ScheduleGenerateBaseAndRail.Invoke(railwayMeshGenerator, new object[] { chunk });
                }
            }
        }

        private static void UpdateSpatialHash(RailTrack track)
        {
            TrackChunkSpatialHash_Field_doneAdding.SetValue(spatialHash, false);

            foreach (KeyValuePair<Vector2Int, List<TrackChunk>> kvp in lookupCells)
                for (int i = kvp.Value.Count - 1; i >= 0; i--)
                    if (kvp.Value[i].track == track)
                        kvp.Value.RemoveAt(i);

            EquiPointSet railPointSet = track.GetUnkinkedPointSet();
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
        }

        private static bool CheckCaches()
        {
            if (cachedValues)
                return true;
            if ((railwayMeshGenerator = Object.FindObjectOfType<RailwayMeshGenerator>()) == null)
                return false;
            if ((spatialHash = (TrackChunkSpatialHash)RailwayMeshGenerator_Field_spatialHash.GetValue(railwayMeshGenerator)) == null)
                return false;
            if ((activeChunks = (Dictionary<Vector2Int, List<TrackChunk>>)RailwayMeshGenerator_Field_activeChunks.GetValue(railwayMeshGenerator)) == null)
                return false;
            if ((lookupCells = (Dictionary<Vector2Int, List<TrackChunk>>)TrackChunkSpatialHash_Field_lookupCells.GetValue(spatialHash)) == null)
                return false;
            cachedValues = true;
            return true;
        }
    }
}
