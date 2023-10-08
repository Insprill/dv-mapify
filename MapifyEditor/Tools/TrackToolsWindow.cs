using Mapify.Editor.Tools.OptionData;
using Mapify.Editor.Utils;
using UnityEditor;
using UnityEngine;
using static Mapify.Editor.Tools.ToolEnums;

#if UNITY_EDITOR
namespace Mapify.Editor.Tools
{
    public partial class TrackToolsWindow : EditorWindow
    {
        [MenuItem("Mapify/Track Tools")]
        public static void ShowWindow()
        {
            var window = GetWindow<TrackToolsWindow>();
            window.Show();
            window.titleContent = new GUIContent("Track Tools");
            window.autoRepaintOnSceneChange = true;
            window._isOpen = true;
            window._updateCounter = 0;
            window.RegisterEvents();
            window.DoNullCheck();
        }

        #region CONSTS

        private const int _updateFrequency = 10;

        #endregion

        #region FIELDS

        // Window.
        private float _scrollMain = 0;
        private bool _isOpen = false;
        private int _updateCounter = 0;

        #endregion

        #region PROPERTIES

        private bool IsLeft => _orientation == TrackOrientation.Left;
        private bool IsInFreeformMode => _showCreation && _creationMode == CreationMode.Freeform;
        private bool IsInPieceMode => _showCreation && _creationMode == CreationMode.Piece;

        public Track CurrentTrack => _selectedTracks.Length > 0 ? _selectedTracks[0] : null;
        public BezierPoint CurrentPoint => _selectedPoints.Length > 0 ? _selectedPoints[0] : null;
        public Switch CurrentSwitch { get; private set; }
        public Turntable CurrentTurntable { get; private set; }

        #endregion

        #region GUI

        private void Awake()
        {
            TryGetDefaultAssets();
        }

        private void OnGUI()
        {
            DoNullCheck();

            _scrollMain = EditorGUILayout.BeginScrollView(new Vector2(0, _scrollMain)).y;

            DrawSelectionFoldout();
            DrawCreationFoldout();
            DrawEditingFoldout();
            DrawPrefabFoldout();
            DrawSettingsFoldout();

            EditorGUILayout.EndScrollView();

            ClampValues();

            // If the GUI has changed, redraw.
            if (GUI.changed)
            {
                RemakeAndRepaint();
            }
        }

        private void OnDestroy()
        {
            _isOpen = false;
            UnregisterEvents();
        }

        private void OnEnable()
        {
            // Make sure the tools still work on project reload.
            _isOpen = HasOpenInstances<TrackToolsWindow>();

            if (_isOpen)
            {
                PrepareSelection();
                RegisterEvents();
            }
            else
            {
                UnregisterEvents();
            }
        }

        private void DoNullCheck()
        {
            // Null checks in case something changes unexpectedly.
            if ((_selectedTracks.Length > 0 && !CurrentTrack) ||
                (_selectedPoints.Length > 0 && !CurrentPoint))
            {
                PrepareSelection();
            }
        }

        private void RegisterEvents()
        {
            SceneView.duringSceneGui += DrawHandles;
            Selection.selectionChanged += PrepareSelection;
            RemakeAndRepaint();
        }

        private void UnregisterEvents()
        {
            SceneView.duringSceneGui -= DrawHandles;
            Selection.selectionChanged -= PrepareSelection;
            RemakeAndRepaint();
        }

        // Called when editor selection changes.
        private void PrepareSelection()
        {
            GameObject go = Selection.activeGameObject;
            CurrentSwitch = null;
            CurrentTurntable = null;

            // Change tools behaviour based on the first selected object.
            if (!go)
            {
                _selectionType = SelectionType.None;
            }
            else if (go.GetComponent<Track>())
            {
                _selectionType = SelectionType.Track;
            }
            else if (go.GetComponent<BezierPoint>())
            {
                _selectionType = SelectionType.BezierPoint;
            }
            else if (go.TryGetComponent(out Switch s))
            {
                _selectionType = SelectionType.Switch;
                CurrentSwitch = s;
            }
            else if (go.TryGetComponent(out Turntable tt))
            {
                _selectionType = SelectionType.Turntable;
                CurrentTurntable = tt;
            }
            else
            {
                _selectionType = SelectionType.None;
            }

            _selectedTracks = Selection.GetFiltered<Track>(
                SelectionMode.TopLevel | SelectionMode.ExcludePrefab | SelectionMode.Editable);
            _selectedPoints = Selection.GetFiltered<BezierPoint>(
                SelectionMode.TopLevel | SelectionMode.ExcludePrefab | SelectionMode.Editable);

            RemakeAndRepaint();
        }

        #endregion

        #region TRACK CREATION

        public void CreateNewTrack()
        {
            if (_currentParent)
            {
                CreateTrack(_currentParent.position, _currentParent.position - _currentParent.forward);
            }
            else
            {
                CreateTrack(Vector3.zero, Vector3.back);
            }
        }

        public void CreateTrack(Vector3 position, Vector3 handle)
        {
            Track t;
            GameObject go;

            switch (_currentPiece)
            {
                case TrackPiece.Straight:
                    t = TrackToolsCreator.CreateStraight(_currentParent, position, handle,
                        _length, _endGrade);
                    ApplySettingsToTrack(t);
                    SelectTrack(t);
                    Undo.RegisterCreatedObjectUndo(t.gameObject, "Created Straight");
                    break;
                case TrackPiece.Curve:
                    t = TrackToolsCreator.CreateArcCurve(_currentParent, position, handle, _orientation,
                        _radius, _arc, _maxArcPerPoint, _endGrade);
                    ApplySettingsToTrack(t);
                    SelectTrack(t);
                    Undo.RegisterCreatedObjectUndo(t.gameObject, "Created Curve");
                    break;
                case TrackPiece.Switch:
                    go = TrackToolsCreator.CreateSwitch(LeftSwitch, RightSwitch, _currentParent, position, handle,
                        _orientation, _connectingPoint).gameObject;
                    SelectGameObject(go);
                    Undo.RegisterCreatedObjectUndo(go, "Created Switch");
                    break;
                case TrackPiece.Yard:
                    go = TrackToolsCreator.CreateYard(LeftSwitch, RightSwitch, _currentParent, position, handle,
                        _orientation, _trackDistance, _yardOptions, out _)[0].gameObject;
                    SelectGameObject(go);
                    Undo.RegisterCreatedObjectUndo(go.transform.parent.gameObject, "Created Yard");
                    break;
                case TrackPiece.Turntable:
                    go = TrackToolsCreator.CreateTurntable(TurntablePrefab, _currentParent, position, handle,
                        _turntableOptions, out Track[] exits).gameObject;
                    SelectGameObject(go);
                    Undo.RegisterCreatedObjectUndo(go, "Created Turntable");
                    int group = Undo.GetCurrentGroup();
                    for (int i = 0; i < exits.Length; i++)
                    {
                        Undo.RegisterCreatedObjectUndo(exits[i].gameObject, "Created turntable exit");
                    }
                    Undo.CollapseUndoOperations(group);
                    break;
                case TrackPiece.Special:
                    CreateSpecial(position, handle);
                    break;
                default:
                    throw new System.Exception("Invalid mode!");
            }
        }

        private void CreateTrack(AttachPoint attachPoint)
        {
            CreateTrack(attachPoint.Position, attachPoint.Handle);
        }

        public void CreateSpecial(Vector3 attachPoint, Vector3 handlePosition)
        {
            Track t;
            GameObject go;

            switch (_currentSpecial)
            {
                case SpecialTrackPiece.Buffer:
                    go = TrackToolsCreator.CreateBuffer(BufferPrefab, _currentParent, attachPoint, handlePosition).gameObject;
                    Undo.RegisterCreatedObjectUndo(go, "Created Buffer Stop");
                    break;
                case SpecialTrackPiece.SwitchCurve:
                    t = TrackToolsCreator.CreateSwitchCurve(LeftSwitch, RightSwitch, _currentParent, attachPoint, handlePosition,
                        _orientation, _connectingPoint);
                    ApplySettingsToTrack(t);
                    SelectTrack(t);
                    Undo.RegisterCreatedObjectUndo(t.gameObject, "Created Switch Curve");
                    break;
                case SpecialTrackPiece.Connect2:
                    CreateConnect2();
                    break;
                case SpecialTrackPiece.Crossover:
                    go = TrackToolsCreator.CreateCrossover(LeftSwitch, RightSwitch, _currentParent, attachPoint, handlePosition,
                        _orientation, _trackDistance, _isTrailing, _switchDistance)[0].gameObject;
                    SelectGameObject(go);
                    Undo.RegisterCreatedObjectUndo(go.transform.parent.gameObject, "Created Crossover");
                    break;
                case SpecialTrackPiece.ScissorsCrossover:
                    go = TrackToolsCreator.CreateScissorsCrossover(LeftSwitch, RightSwitch, _currentParent, attachPoint, handlePosition,
                        _orientation, _trackDistance, _switchDistance)[3].gameObject;
                    SelectGameObject(go);
                    Undo.RegisterCreatedObjectUndo(go.transform.parent.gameObject, "Created Scissors Crossover");
                    break;
                case SpecialTrackPiece.DoubleSlip:
                    go = TrackToolsCreator.CreateDoubleSlip(LeftSwitch, RightSwitch, _currentParent, attachPoint, handlePosition,
                        _orientation, _crossAngle)[2].gameObject;
                    SelectGameObject(go);
                    Undo.RegisterCreatedObjectUndo(go.transform.parent.gameObject, "Created Double Slip");
                    break;
                default:
                    throw new System.Exception("Invalid mode!");
            }
        }

        private void CreateConnect2()
        {
            Track t;

            switch (_selectionType)
            {
                case SelectionType.Track:
                    BezierPoint p0 = _useHandle2Start ? _selectedTracks[0].Curve[0] : _selectedTracks[0].Curve.Last();
                    BezierPoint p1 = _useHandle2End ? _selectedTracks[1].Curve[0] : _selectedTracks[1].Curve.Last();

                    t = TrackToolsCreator.CreateConnect2Point(_currentParent, p0, p1,
                        _useHandle2Start, _useHandle2End, _lengthMultiplier);
                    ApplySettingsToTrack(t);
                    SelectTrack(t);
                    Undo.RegisterCreatedObjectUndo(t.gameObject, "Created Connect 2");
                    break;
                case SelectionType.BezierPoint:
                    t = TrackToolsCreator.CreateConnect2Point(_currentParent, _selectedPoints[0], _selectedPoints[1],
                                _useHandle2Start, _useHandle2End, _lengthMultiplier);
                    ApplySettingsToTrack(t);
                    SelectTrack(t);
                    Undo.RegisterCreatedObjectUndo(t.gameObject, "Created Connect 2");
                    break;
                default:
                    break;
            }
        }

        public void DeleteTrack()
        {
            switch (_selectionType)
            {
                case SelectionType.Track:
                    if (!CurrentTrack)
                    {
                        return;
                    }

                    if (CurrentTrack.IsSwitch || CurrentTrack.IsTurntable)
                    {
                        Undo.DestroyObjectImmediate(CurrentTrack.transform.parent.gameObject);
                    }
                    else
                    {
                        Undo.DestroyObjectImmediate(CurrentTrack.gameObject);
                    }
                    break;
                case SelectionType.Switch:
                    if (!CurrentSwitch)
                    {
                        return;
                    }

                    Undo.DestroyObjectImmediate(CurrentSwitch.gameObject);
                    break;
                case SelectionType.Turntable:
                    if (!CurrentTurntable)
                    {
                        return;
                    }

                    Undo.DestroyObjectImmediate(CurrentTurntable.gameObject);
                    break;
                default:
                    break;
            }

            PrepareSelection();
        }

        #endregion

        #region OTHER

        /// <summary>
        /// Tries to assign the default Mapify assets to the prefab section.
        /// </summary>
        /// <remarks>
        /// This will only look in the default directory (Mapify folder in the Assets root).
        /// </remarks>
        public void TryGetDefaultAssets()
        {
            Track t = null;
            TrackToolsHelper.TryGetDefaultPrefabs(ref t, ref BufferPrefab, ref LeftSwitch, ref RightSwitch, ref TurntablePrefab);
        }

        public void ResetCreationSettings(bool all)
        {
            _endGrade = 0.0f;
            _heightOffset = 0.5f;
            _smoothMix = 1.0f;

            if (all || _currentPiece == TrackPiece.Straight)
            {
                _length = 100;
            }
            if (all || _currentPiece == TrackPiece.Curve)
            {
                _radius = 500.0f;
                _arc = 45.0f;
                _maxArcPerPoint = 22.5f;
                _changeArc = false;
            }
            if (all || _currentPiece == TrackPiece.Switch)
            {
                _connectingPoint = SwitchPoint.Joint;
            }
            if (all || _currentPiece == TrackPiece.Yard)
            {
                _yardOptions = YardOptions.DefaultOptions;
            }
            if (all || _currentPiece == TrackPiece.Turntable)
            {
                _turntableOptions = TurntableOptions.DefaultOptions;
            }
            if (all || _currentPiece == TrackPiece.Special)
            {
                _useHandle2Start = false;
                _useHandle2End = false;
                _lengthMultiplier = 1.0f;
                _switchDistance = 6.0f;
                _crossAngle = 20.0f;
            }

            RemakeAndRepaint();
        }

        public void ResetPreviewSettings()
        {
            _performanceMode = false;
            _updateCounter = 0;
            _forwardColour = Color.cyan;
            _backwardColour = Color.red;
            _newColour = Color.green;
            _sampleCount = 8;

            RemakeAndRepaint();
        }

        private bool IsAllowedCreation(bool isBehind, out string tooltip)
        {
            switch (_selectionType)
            {
                case SelectionType.Track:
                    if (!CurrentTrack)
                    {
                        tooltip = "No selection";
                        return false;
                    }

                    if (!CheckGrade(isBehind ? CurrentTrack.GetGradeAtStart() : CurrentTrack.GetGradeAtEnd()))
                    {
                        tooltip = "Grade too steep for creation";
                        return false;
                    }

                    if (CurrentTrack.IsSwitch && (_currentPiece == TrackPiece.Switch || _currentPiece == TrackPiece.Yard))
                    {
                        tooltip = "Cannot attach a switch to another switch directly";
                        return false;
                    }

                    if (_currentPiece == TrackPiece.Special && _currentSpecial == SpecialTrackPiece.Connect2)
                    {
                        tooltip = "Use the [New Track] button for this feature";
                        return false;
                    }

                    tooltip = isBehind ? "Creates a track behind the current one" : "Creates a track in front of the current one";
                    return true;
                case SelectionType.BezierPoint:
                    if (!CurrentPoint)
                    {
                        tooltip = "No selection";
                        return false;
                    }

                    if (!CheckGrade(isBehind ? CurrentPoint.GetGradeBackwards() : CurrentPoint.GetGradeForwards()))
                    {
                        tooltip = "Grade too steep for creation";
                        return false;
                    }

                    if (_currentPiece == TrackPiece.Special && _currentSpecial == SpecialTrackPiece.Connect2)
                    {
                        tooltip = "Use the [New Track] button for this feature";
                        return false;
                    }

                    tooltip = isBehind ? "Creates a track behind the current point" : "Creates a track in front of the current point";
                    return true;
                default:
                    tooltip = "Selection is not valid for creation";
                    return false;
            }
        }

        private bool IsAllowedCreation(Track t, bool isBack)
        {
            if (!t)
            {
                return false;
            }

            if (!CheckGrade(isBack ? t.GetGradeAtStart() : t.GetGradeAtEnd()))
            {
                return false;
            }

            if (t.IsSwitch && (_currentPiece == TrackPiece.Switch || _currentPiece == TrackPiece.Yard))
            {
                return false;
            }

            if (_currentPiece == TrackPiece.Special && _currentSpecial == SpecialTrackPiece.Connect2)
            {
                return false;
            }

            return true;
        }

        private bool CheckGrade(float grade)
        {
            switch (_currentPiece)
            {
                case TrackPiece.Switch:
                case TrackPiece.Yard:
                case TrackPiece.Turntable:
                    return Mathf.Approximately(grade, 0);
                case TrackPiece.Special:
                    switch (_currentSpecial)
                    {
                        case SpecialTrackPiece.SwitchCurve:
                        case SpecialTrackPiece.Crossover:
                        case SpecialTrackPiece.ScissorsCrossover:
                            return Mathf.Approximately(grade, 0);
                        default:
                            return true;
                    }
                default:
                    return true;
            }
        }

        private void SelectTrack(Track t)
        {
            SelectGameObject(t.gameObject);
        }

        private void SelectGameObject(GameObject go)
        {
            Selection.activeGameObject = go;
        }

        private void ClampValues()
        {
            _length = Mathf.Max(_length, 0);

            _radius = Mathf.Max(_radius, 0);
            _arc = Mathf.Clamp(_arc, 0, 180);
            _maxArcPerPoint = Mathf.Clamp(_maxArcPerPoint, 1, 90);

            _lengthMultiplier = Mathf.Max(_lengthMultiplier, 0);
            _switchDistance = Mathf.Max(_switchDistance, 0);

            _sampleCount = Mathf.Clamp(_sampleCount, 2, 64);
        }

        private void RemakeAndRepaint()
        {
            DoNullCheck();

            // Force a GUI redraw too so a selection change is reflected right away.
            Repaint();
            SceneView.RepaintAll();
        }

        private Switch GetCurrentSwitchPrefab()
        {
            return IsLeft ? LeftSwitch : RightSwitch;
        }

        private Switch GetSwitch(TrackOrientation orientation)
        {
            if (orientation == TrackOrientation.Left)
            {
                return LeftSwitch;
            }
            else
            {
                return RightSwitch;
            }
        }

        // Check if an object is not null, draw an error box if it is.
        private static bool Require(Object obj, string name)
        {
            if (obj)
            {
                return true;
            }

            EditorGUILayout.HelpBox($"{name} must be assigned to use this function.", MessageType.Error);
            return false;
        }

        private void NotImplementedGUI()
        {
            EditorGUILayout.HelpBox("Not implemented yet!", MessageType.Warning);
        }

        private void ApplySettingsToTrack(Track t)
        {
            t.transform.parent = _currentParent;
            t.age = _trackAge;
            t.generateSigns = _generateSigns;
            t.generateBallast = _generateBallast;
            t.generateSleepers = _generateSleepers;
        }

        #endregion

        #region DATA STRUCTURES

        private struct AttachPoint
        {
            public Vector3 Position;
            public Vector3 Handle;

            public Vector3 NegativeHandle => Position - (Handle - Position);

            public AttachPoint(Vector3 position, Vector3 handle)
            {
                Position = position;
                Handle = handle;
            }

            public float GetGrade() => MathHelper.GetGrade(Position, Handle);

            public static AttachPoint FromBezierPoint(BezierPoint bp, bool isBehind)
            {
                if (isBehind)
                {
                    return new AttachPoint(bp.position, bp.globalHandle2);
                }
                else
                {
                    return new AttachPoint(bp.position, bp.globalHandle1);
                }
            }
        }

        private class PreviewPointCache
        {
            public AttachPoint Attach;
            public Vector3[][] Lines;
            public Vector3[] Points;
            public bool DrawButton;
            public string Tooltip;
            public bool AllowGUI;

            public static string NewString => "★";
            public static string NextString => "→";
            public static string BackString => "←";
            public static string DivString => "↗";

            public PreviewPointCache(AttachPoint attach)
            {
                Attach = attach;
                Lines = System.Array.Empty<Vector3[]>();
                Points = System.Array.Empty<Vector3>();
                DrawButton = true;
                Tooltip = "";
                AllowGUI = true;
            }

            public void DrawLines()
            {
                for (int i = 0; i < Lines.Length; i++)
                {
                    Handles.DrawPolyLine(Lines[i]);
                }
            }

            public void DrawPointsGUI(SceneView scene)
            {
                if (DrawButton)
                {
                    Handles.DrawWireDisc(HandleUtility.WorldToGUIPoint(Attach.Position), Vector3.forward, 15.0f);
                    // TODO: use a disc instead of this so it is not selectable.
                    //Handles.FreeRotateHandle(Quaternion.identity, Attach.Position,
                    //        HandleUtility.GetHandleSize(Attach.Position) * 0.15f);
                }

                for (int i = 0; i < Points.Length; i++)
                {
                    Handles.DrawWireDisc(HandleUtility.WorldToGUIPoint(Points[i]), Vector3.forward, 5.0f);
                    //Handles.FreeRotateHandle(Quaternion.identity, Points[i],
                    //    HandleUtility.GetHandleSize(Points[i]) * 0.05f);
                }
            }
        }

        private class FreeformTrackHelper
        {
            public Vector3 StartNormal;
            public Vector3? Start;
            public Vector3? StartHandle;
            public Vector3? Next;
            public Vector3? NextHandle;
            public Track WorkingTrack;
            public int? UndoIndex;
            public bool Locked;

            public bool HasStart => Start.HasValue;
            public bool IsStartSnapped => StartHandle.HasValue;
            public bool IsNextSnapped => Next.HasValue;

            public FreeformTrackHelper(Vector3 normal)
            {
                StartNormal = normal;
                Start = null;
                StartHandle = null;
                Next = null;
                NextHandle = null;
                WorkingTrack = null;
                UndoIndex = null;
                Locked = false;
            }

            public void CheckForSnap(Vector3 v, float radius)
            {
                Vector3 pos;
                Vector3 hand;
                bool snapped = TrackToolsHelper.CheckForTrackSnap(v, radius, out pos, out hand);

                // Start has a value, so we are working with new points.
                if (Start.HasValue)
                {
                    if (snapped)
                    {
                        NextHandle = hand;
                    }
                    else
                    {
                        NextHandle = null;
                    }

                    Next = pos;

                    return;
                }

                if (snapped)
                {
                    StartHandle = hand;
                }
                else
                {
                    StartHandle = null;
                }

                Start = pos;
            }

            public AttachPoint ToAttachPoint()
            {
                if (WorkingTrack)
                {
                    return AttachPoint.FromBezierPoint(WorkingTrack.Curve.Last(), false);
                }

                if (!Start.HasValue)
                {
                    throw new System.Exception("Start is null!");
                }

                if (StartHandle.HasValue)
                {
                    return new AttachPoint(Start.Value, StartHandle.Value);
                }

                return new AttachPoint(Start.Value, Start.Value);
            }
        }

        #endregion
    }
}
#endif
