using System.Reflection;
using HarmonyLib;
using Mapify.Editor.Utils;
using Mapify.Utils;
using UnityEngine;
using UnityModManagerNet;

namespace Mapify.Components
{
    public class RailwayBuilder : MonoBehaviour
    {
        private const float MAX_DIST = 100.0f;
        private const float CONNECT_THRESHOLD = 4f;

        private static readonly FieldInfo RailTrack_Field_pointSet = AccessTools.DeclaredField(typeof(RailTrack), "pointSet");

        private KeyBinding keyBinding;
        private bool isPlacing;
        private RailTrack beingPlaced;

        private void Awake()
        {
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
            RailwayMeshUpdater.UpdateTrack(beingPlaced);
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
            RailwayMeshUpdater.UpdateTrack(beingPlaced);
        }
    }
}
