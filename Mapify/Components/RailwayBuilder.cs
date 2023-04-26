using System.Collections.Generic;
using System.Reflection;
using DV;
using DV.PointSet;
using HarmonyLib;
using Mapify.Editor.Utils;
using Mapify.Utils;
using Unity.Jobs;
using UnityEngine;
using UnityModManagerNet;

namespace Mapify.Components
{
    public class RailwayBuilder : MonoBehaviour
    {
        private const float MAX_DIST = 100.0f;
        private const float CONNECT_THRESHOLD = 4f;

        private static readonly FieldInfo RailwayMeshGenerator_Field_spatialHash = AccessTools.DeclaredField(typeof(RailwayMeshGenerator), "spatialHash");
        private static readonly FieldInfo RailwayMeshGenerator_Field_activeChunks = AccessTools.DeclaredField(typeof(RailwayMeshGenerator), "activeChunks");
        private static readonly FieldInfo RailwayMeshGenerator_Field_activeJobs = AccessTools.DeclaredField(typeof(RailwayMeshGenerator), "activeJobs");
        private static readonly FieldInfo RailwayMeshGenerator_Field_prevCellId = AccessTools.DeclaredField(typeof(RailwayMeshGenerator), "prevCellId");
        private static readonly FieldInfo TrackChunkSpatialHash_Field_doneAdding = AccessTools.DeclaredField(typeof(TrackChunkSpatialHash), "doneAdding");
        private static readonly FieldInfo TrackChunkSpatialHash_Field_lookupCells = AccessTools.DeclaredField(typeof(TrackChunkSpatialHash), "lookupCells");
        private static readonly FieldInfo RailTrack_Field_pointSet = AccessTools.DeclaredField(typeof(RailTrack), "pointSet");

        private RailwayMeshGenerator railwayMeshGenerator;
        private KeyBinding keyBinding;
        private bool isPlacing;
        private RailTrack beingPlaced;

        private void Awake()
        {
            railwayMeshGenerator = FindObjectOfType<RailwayMeshGenerator>();
            keyBinding = new KeyBinding();
            keyBinding.Change(KeyCode.T, false, true, false);
        }

        private void Update()
        {
            bool isPressed = keyBinding.Down();
            switch (isPlacing)
            {
                case false when isPressed:
                    StartPlacing();
                    break;
                case true when isPressed:
                    FinishPlacing();
                    break;
                case true:
                    UpdatePlacing();
                    break;
            }
        }

        private void StartPlacing()
        {
            Transform camera = PlayerManager.PlayerCamera.transform;
            Vector3 cameraForward = camera.forward;
            if (!Physics.Raycast(camera.position, cameraForward, out RaycastHit hit, MAX_DIST))
                return;

            isPlacing = true;
            BezierPoint point = hit.point.GetClosestComponent<BezierPoint>();
            Vector3 startPos;
            if (point != null && Vector3.Distance(point.transform.position, hit.point) < CONNECT_THRESHOLD)
            {
                int idx = point.curve.GetPointIndex(point);
                if (idx != 0 && idx != point.curve.pointCount - 1)
                    return; //todo: bug?
                startPos = point.position;
            }
            else
            {
                startPos = hit.point.AddY(1.0f);
            }

            GameObject go = WorldMover.Instance.NewChild("track");
            go.SetActive(false);
            BezierCurve curve = go.AddComponent<BezierCurve>();
            curve.resolution = 0.5f;
            curve.AddPointAt(startPos);
            curve.AddPointAt(startPos + cameraForward.AddY(-cameraForward.y).normalized * 2.0f);
            RailTrack railTrack = go.AddComponent<RailTrack>();
            railTrack.dontChange = false;
            railTrack.ApplyRailType();
            go.SetActive(true);
            beingPlaced = railTrack;
        }

        private void FinishPlacing()
        {
            beingPlaced.ConnectInToClosestBranch();
            beingPlaced.ConnectOutToClosestBranch();
            if (beingPlaced.generateColliders)
                beingPlaced.CreateCollider();
            AddOrUpdateTrack(railwayMeshGenerator, beingPlaced);
            beingPlaced = null;
            isPlacing = false;
        }

        private void UpdatePlacing()
        {
            Camera camera = PlayerManager.PlayerCamera;
            if (!Physics.Raycast(camera.transform.position, camera.transform.forward, out RaycastHit hit, MAX_DIST))
                return;
            Vector3 pos = hit.point.AddY(1);
            if (beingPlaced.curve.Last().position == pos)
                return;
            if (Vector3.Distance(beingPlaced.curve[0].position, pos) < 2.0f)
                return;
            beingPlaced.curve.Last().position = pos;
            RailTrack.pointSets.Remove(beingPlaced);
            RailTrack.pointSets.Add(beingPlaced, beingPlaced.GetPointSet());
            RailTrack_Field_pointSet.SetValue(beingPlaced, null);
            AddOrUpdateTrack(railwayMeshGenerator, beingPlaced);
        }

        // todo: there's a leak somewhere that causes performance issues
        private static void AddOrUpdateTrack(RailwayMeshGenerator railwayMeshGenerator, RailTrack track)
        {
            TrackChunkSpatialHash spatialHash = (TrackChunkSpatialHash)RailwayMeshGenerator_Field_spatialHash.GetValue(railwayMeshGenerator);
            TrackChunkSpatialHash_Field_doneAdding.SetValue(spatialHash, false);

            Dictionary<Vector2Int, List<TrackChunk>> lookupCells = (Dictionary<Vector2Int, List<TrackChunk>>)TrackChunkSpatialHash_Field_lookupCells.GetValue(spatialHash);
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
            Dictionary<Vector2Int, List<TrackChunk>> activeChunks = (Dictionary<Vector2Int, List<TrackChunk>>)RailwayMeshGenerator_Field_activeChunks.GetValue(railwayMeshGenerator);
            foreach (KeyValuePair<Vector2Int, List<TrackChunk>> kvp in activeChunks)
            foreach (TrackChunk chunk in kvp.Value)
                if (chunk.track == track)
                    chunk.ReleasePoolObjects();
            activeChunks.Clear();

            List<(MeshSweeperJob meshSweeperJob, JobHandle jobHandle, Mesh mesh)> activeJobs =
                (List<(MeshSweeperJob meshSweeperJob, JobHandle jobHandle, Mesh mesh)>)RailwayMeshGenerator_Field_activeJobs.GetValue(railwayMeshGenerator);
            activeJobs.Clear();
        }
    }
}
