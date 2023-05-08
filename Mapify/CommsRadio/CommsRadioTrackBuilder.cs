using System.Reflection;
using DV;
using HarmonyLib;
using Mapify.Editor.Utils;
using Mapify.Patches;
using Mapify.Utils;
using UnityEngine;

namespace Mapify.Components
{
    public class CommsRadioTrackBuilder : MonoBehaviour, ICommsRadioMode
    {
        private const string MODE_NAME = "Build Track";
        private const string MAIN_MENU_CONTENT = "Place trackage?";
        private const float MAX_RANGE = 150.0f;
        private const float Y_OFFSET = 1.0f;
        private const float SNAP_DIST = 2.0f;

        private static readonly Color LASER_COLOR = new Color32(255, 255, 255, 255);
        private static readonly FieldInfo RailTrack_Field_pointSet = AccessTools.DeclaredField(typeof(RailTrack), "pointSet");

        private State state;
        private CommsRadioDisplay display;
        private Transform signalOrigin;

        private Option<Vector3> targetPosition;
        private Option<RailTrack> placingTrack;

        public void Awake()
        {
            CommsRadioController controller = CommsRadioController_Awake_Patch.controller;
            display = controller.carSpawnerControl.display;
        }

        public void Enable()
        {
            SetState(State.MainMenu);
        }

        public void Disable()
        {
            ClearFlags();
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
                    RailTrack_Field_pointSet.SetValue(track, null);
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

        public ButtonBehaviourType ButtonBehaviour { get; }

        private void SetState(State newState)
        {
            state = newState;
            switch (state)
            {
                case State.MainMenu:
                    display.SetDisplay(MODE_NAME);
                    break;
                case State.PlaceTrack:
                    display.SetContentAndAction("Place track?");
                    break;
                case State.FinishTrack:
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
                            display.SetContentAndAction("Track"); //todo
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
                            GameObject go = WorldMover.Instance.NewChild("track");
                            go.SetActive(false);
                            BezierCurve curve = go.AddComponent<BezierCurve>();
                            curve.resolution = 0.5f;
                            curve.AddPointAt(pos);
                            curve.AddPointAt(pos + signalOrigin.forward.AddY(-signalOrigin.forward.y).normalized * 2.0f);
                            RailTrack track = go.AddComponent<RailTrack>();
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

        private void ClearFlags()
        {
            SetState(State.MainMenu);
            targetPosition = Vector3.zero;
            if (placingTrack.TakeIfSome(out RailTrack track))
                Destroy(track.gameObject);
        }

        private void UpdateTargetPos()
        {
            Ray ray = new Ray(signalOrigin.position, signalOrigin.forward);
            if (!Physics.Raycast(ray, out RaycastHit hit, MAX_RANGE))
            {
                targetPosition = Option<Vector3>.None;
                return;
            }

            BezierPoint point = hit.point.GetClosestComponent<BezierPoint>();
            if (point != null && Vector3.Distance(point.transform.position, hit.point) < SNAP_DIST)
            {
                int idx = point.curve.GetPointIndex(point);
                targetPosition = idx != 0 && idx != point.curve.pointCount - 1 ? point.position : hit.point.AddY(Y_OFFSET);
            }
            else
            {
                targetPosition = hit.point.AddY(Y_OFFSET);
            }
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
