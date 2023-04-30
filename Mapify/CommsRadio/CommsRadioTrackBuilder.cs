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
        private const float Y_OFFSET = 2.0f;
        private const float SNAP_DIST = 2.0f;

        private static readonly Color LASER_COLOR = new Color32(255, 255, 255, 255);
        private static readonly FieldInfo RailTrack_Field_pointSet = AccessTools.DeclaredField(typeof(RailTrack), "pointSet");

        private State state;
        private CommsRadioDisplay display;
        private Transform signalOrigin;

        private Vector3 position;
        private RailTrack placingTrack;

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
            {
                Ray ray = new Ray(signalOrigin.position, signalOrigin.forward);
                if (Physics.Raycast(ray, out RaycastHit hit, MAX_RANGE))
                {
                    position = hit.point.AddY(Y_OFFSET);
                    BezierPoint point = hit.point.GetClosestComponent<BezierPoint>();
                    if (point != null && Vector3.Distance(point.transform.position, hit.point) < SNAP_DIST)
                    {
                        int idx = point.curve.GetPointIndex(point);
                        position = idx != 0 && idx != point.curve.pointCount - 1 ? point.position : hit.point.AddY(Y_OFFSET);
                    }
                    else
                    {
                        position = hit.point.AddY(Y_OFFSET);
                    }
                }
            }

            switch (state)
            {
                case State.PlaceTrack:
                case State.FinishTrack:
                    Camera camera = PlayerManager.PlayerCamera;
                    if (!Physics.Raycast(camera.transform.position, camera.transform.forward, out RaycastHit hit, MAX_RANGE))
                        return;
                    Vector3 pos = hit.point.AddY(1);
                    if (placingTrack.curve.Last().position == pos)
                        return;
                    if (Vector3.Distance(placingTrack.curve[0].position, pos) < 2.0f)
                        return;
                    placingTrack.curve.Last().position = pos;
                    RailTrack.pointSets.Remove(placingTrack);
                    RailTrack.pointSets.Add(placingTrack, placingTrack.GetPointSet());
                    RailTrack_Field_pointSet.SetValue(placingTrack, null);
                    RailwayMeshUpdater.UpdateTrack(placingTrack);
                    break;
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
                    // display.SetDisplay(MODE_NAME);
                    break;
                case State.PlaceTrack:
                    // display.SetDisplay(MODE_NAME, "Place track?");
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
                        case CommsRadioAction.Use:
                            GameObject go = WorldMover.Instance.NewChild("track");
                            go.SetActive(false);
                            BezierCurve curve = go.AddComponent<BezierCurve>();
                            curve.resolution = 0.5f;
                            curve.AddPointAt(position);
                            curve.AddPointAt(position + signalOrigin.forward.AddY(-signalOrigin.forward.y).normalized * 2.0f);
                            placingTrack = go.AddComponent<RailTrack>();
                            placingTrack.dontChange = false;
                            placingTrack.ApplyRailType();
                            go.SetActive(true);
                            return State.FinishTrack;
                        case CommsRadioAction.Decrease:
                            placingTrack.transform.Rotate(Vector3.up, -15.0f);
                            break;
                        case CommsRadioAction.Increase:
                            placingTrack.transform.Rotate(Vector3.up, 15.0f);
                            break;
                    }

                    break;
                }
                case State.FinishTrack: {
                    switch (action)
                    {
                        case CommsRadioAction.Use:
                            placingTrack.ConnectInToClosestBranch();
                            placingTrack.ConnectOutToClosestBranch();
                            if (placingTrack.generateColliders)
                                placingTrack.CreateCollider();
                            RailwayMeshUpdater.UpdateTrack(placingTrack);
                            placingTrack = null;
                            return State.MainMenu;
                        case CommsRadioAction.Decrease:
                            placingTrack.transform.Rotate(Vector3.up, -15.0f);
                            break;
                        case CommsRadioAction.Increase:
                            placingTrack.transform.Rotate(Vector3.up, 15.0f);
                            break;
                    }

                    break;
                }
            }

            return state;
        }

        private void ClearFlags()
        {
            SetState(State.MainMenu);
            position = Vector3.zero;
            if (placingTrack != null)
                Destroy(placingTrack.gameObject);
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
