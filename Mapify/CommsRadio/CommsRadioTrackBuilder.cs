using System.Linq;
using DV;
using Mapify.Editor.Utils;
using Mapify.Patches;
using Mapify.Utils;
using UnityEngine;

namespace Mapify.Components
{
    //TODO should we use the comms radio API? https://github.com/fauxnik/dv-comms-radio-api/
    public class CommsRadioTrackBuilder : MonoBehaviour, ICommsRadioMode
    {
        private const string MODE_NAME = "Build Track";
        private const string MAIN_MENU_CONTENT = "Place trackage?";
        private const float MAX_RANGE = 150.0f;
        private const float Y_OFFSET = 1.0f;
        private const float SNAP_DIST = 2.0f;

        private static readonly Color LASER_COLOR = new Color32(255, 255, 255, 255);

        private State state;
        private CommsRadioDisplay display;
        private Transform signalOrigin;

        private Option<Vector3> targetPosition;
        private Option<RailTrack> placingTrack;

        private GameObject previewObj;
        private MeshRenderer previewObjMesh;

        public void Awake()
        {
            var controller = CommsRadioController_Awake_Patch.Controller;
            display = controller.carSpawnerControl.display;

            ButtonBehaviour = ButtonBehaviourType.Regular;
        }

        public void Enable()
        {
            SetState(State.MainMenu);
        }

        public void Disable()
        {
            SetState(State.MainMenu);
            targetPosition = Vector3.zero;
            if (placingTrack.TakeIfSome(out RailTrack track))
                Destroy(track.gameObject);
            Destroy(previewObj);
        }

        public void OverrideSignalOrigin(Transform origin)
        {
            signalOrigin = origin;
        }

        public void OnUse()
        {
            SetState(OnAction(CommsRadioAction.Use));
        }

        public void OnUpdate()
        {
            if (state != State.MainMenu && state != State.SelectObject)
                UpdateTargetPos();

            switch (state)
            {
                case State.PlaceTrack: {
                    if (!placingTrack.IsSome(out RailTrack track))
                        return;
                    if (!targetPosition.IsSome(out Vector3 pos))
                        return;
                    Vector3 forward = signalOrigin.forward;
                    track.curve[0].position = pos;
                    track.curve.Last().position = pos + forward.AddY(-forward.y).normalized * 2.0f;
                    break;
                }
                case State.FinishTrack: {

                    if (!placingTrack.IsSome(out RailTrack track))
                        return;
                    if (!targetPosition.IsSome(out Vector3 pos))
                        return;
                    if (track.curve.Last().position == pos)
                        return;
                    if (Vector3.Distance(track.curve[0].position, pos) < 2.0f)
                        return;
                    track.curve.Last().position = pos;
                    RailTrack.pointSets.Remove(track);
                    RailTrack.pointSets.Add(track, track.GetPointSet());
                    track.pointSet = null;
                    Destroy(previewObj); //TODO this doesn't seem to work, the previewObj stays
                    RailwayMeshUpdater.UpdateTrack(track);
                    break;
                }
            }
        }

        public bool ButtonACustomAction()
        {
            SetState(OnAction(CommsRadioAction.Decrease));
            return true;
        }

        public bool ButtonBCustomAction()
        {
            SetState(OnAction(CommsRadioAction.Increase));
            return true;
        }

        public void SetStartingDisplay()
        {
            display.SetDisplay(MODE_NAME, MAIN_MENU_CONTENT);
        }

        public Color GetLaserBeamColor()
        {
            return LASER_COLOR;
        }

        public ButtonBehaviourType ButtonBehaviour { get; private set; }

        private void SetState(State newState)
        {
            state = newState;
            switch (state)
            {
                case State.MainMenu:
                    ButtonBehaviour = ButtonBehaviourType.Regular;
                    display.SetDisplay(MODE_NAME);
                    break;
                case State.PlaceTrack:
                    ButtonBehaviour = ButtonBehaviourType.Override;
                    display.SetContentAndAction("Place track?");
                    break;
                case State.FinishTrack:
                    ButtonBehaviour = ButtonBehaviourType.Override;
                    display.SetContentAndAction("Finish placing track?");
                    break;
            }
        }

        private State OnAction(CommsRadioAction action)
        {
            switch (state)
            {
                case State.MainMenu: {
                    switch (action)
                    {
                        case CommsRadioAction.Use:
                            display.SetContentAndAction("Straight track");
                            return State.SelectObject;
                    }

                    break;
                }
                case State.SelectObject: {
                    switch (action)
                    {
                        case CommsRadioAction.Use:
                            return State.PlaceTrack; //todo
                        case CommsRadioAction.Decrease:
                            break;
                        case CommsRadioAction.Increase:
                            break;
                    }

                    break;
                }
                case State.PlaceTrack: {
                    switch (action)
                    {
                        case CommsRadioAction.Use: {
                            if (!targetPosition.IsSome(out Vector3 pos))
                                return state;

                            var go = WorldData.Instance.TrackRootParent.NewChild("track");
                            go.SetActive(false);

                            var curve = go.AddComponent<BezierCurve>();
                            curve.resolution = 0.5f; //TODO make this a constant or get it from the base game
                            curve.AddPointAt(pos);

                            //without this, BezierCurveUpgrade will increase the resolution and RailTrack will decrease it again.
                            curve.version = 2;

                            curve.AddPointAt(pos + signalOrigin.forward.AddY(-signalOrigin.forward.y).normalized * 2.0f);

                            var track = go.AddComponent<RailTrack>();
                            track.dontChange = false;
                            track.ApplyRailType();
                            placingTrack = track;

                            go.SetActive(true);
                            return State.FinishTrack;
                        }
                        case CommsRadioAction.Decrease: {
                            if (placingTrack.IsSome(out RailTrack track))
                                track.transform.Rotate(Vector3.up, -15.0f);
                            break;
                        }
                        case CommsRadioAction.Increase: {
                            if (placingTrack.IsSome(out RailTrack track))
                                track.transform.Rotate(Vector3.up, 15.0f);
                            break;
                        }
                    }

                    break;
                }
                case State.FinishTrack: {
                    switch (action)
                    {
                        case CommsRadioAction.Use: {
                            if (!placingTrack.TakeIfSome(out RailTrack track))
                                return state;
                            track.ConnectInToClosestBranch();
                            track.ConnectOutToClosestBranch();
                            if (track.generateColliders)
                                track.CreateCollider();
                            RailwayMeshUpdater.UpdateTrack(track);
                            return State.MainMenu;
                        }
                        case CommsRadioAction.Decrease: {
                            if (!placingTrack.IsSome(out RailTrack track))
                                return state;
                            track.transform.Rotate(Vector3.up, -15.0f);
                            break;
                        }
                        case CommsRadioAction.Increase: {
                            if (!placingTrack.IsSome(out RailTrack track))
                                return state;
                            track.transform.Rotate(Vector3.up, 15.0f);
                            break;
                        }
                    }

                    break;
                }
            }

            return state;
        }

        private void UpdateTargetPos()
        {
            var ray = new Ray(signalOrigin.position, signalOrigin.forward);
            if (!Physics.Raycast(ray, out RaycastHit hit, MAX_RANGE))
            {
                Destroy(previewObj);
                targetPosition = Option<Vector3>.None;
                return;
            }

            if (previewObj == null)
            {
                previewObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                previewObj.name = "previewObj";
                Destroy(previewObj.GetComponent<SphereCollider>());
                previewObjMesh = previewObj.GetComponent<MeshRenderer>();
                var standardShader = new Material(Shader.Find("Standard"));
                previewObjMesh.material = standardShader;
            }

            // snap to an existing track
            BezierPoint closestExistingPoint;
            if (placingTrack.IsSome(out RailTrack track))
            {
                closestExistingPoint = FindObjectsOfType<BezierPoint>()
                    .Where(point => !track._curve.points.Contains(point))
                    .OrderBy(point => (hit.point - point.transform.position).sqrMagnitude)
                    .FirstOrDefault();
            }
            else
            {
                closestExistingPoint = hit.point.GetClosestComponent<BezierPoint>();
            }

            if (closestExistingPoint != null && Vector3.Distance(closestExistingPoint.transform.position, hit.point) < SNAP_DIST)
            {
                targetPosition = closestExistingPoint.position;
                previewObjMesh.material.color = Color.green;
            }
            else
            {
                targetPosition = hit.point.AddY(Y_OFFSET);
                previewObjMesh.material.color = Color.red;
            }

            previewObj.transform.position = targetPosition.value;
        }

        private enum State
        {
            MainMenu,
            SelectObject,
            PlaceTrack,
            FinishTrack,
            PlaceSwitch,
            PlaceTurntable
        }
    }
}
