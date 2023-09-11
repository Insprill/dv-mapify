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
            window.RegisterEvents(true);
            window.DoNullCheck();
        }

        #region FIELDS

        // Window.
        private float _scrollMain = 0;
        private bool _isOpen = false;
        private GameObject _lastSelection = null;

        // Drawing.
        private Vector3[] _forwardPoints = new Vector3[0];
        private Vector3[] _newPoints = new Vector3[0];
        private Vector3[] _backwardPoints = new Vector3[0];
        private Vector3[][] _forwardLines = new Vector3[0][];
        private Vector3[][] _newLines = new Vector3[0][];
        private Vector3[][] _backwardLines = new Vector3[0][];

        #endregion

        #region PROPERTIES

        private bool _isLeft => _orientation == TrackOrientation.Left;
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

        private void OnInspectorUpdate()
        {
            if (!_isOpen)
            {
                return;
            }

            _updateCounter = (_updateCounter + 1) % 10;

            if (!_performanceMode || _updateCounter % 10 == 0)
            {
                // Only check if the window is closed if the last state is open.
                if (_isOpen && !HasOpenInstances<TrackToolsWindow>())
                {
                    _isOpen = false;
                    UnregisterEvents();
                    return;
                }

                // If selection changed, draw. Also draw if the selection is of a supported type.
                if ((_lastSelection && _lastSelection != Selection.activeGameObject) ||
                    _selectionType != SelectionType.None)
                {
                    RemakeAndRepaint();
                    _lastSelection = Selection.activeGameObject;
                }
            }
        }

        private void OnDestroy()
        {
            _isOpen = false;
            UnregisterEvents();
        }

        private void OnSelectionChange()
        {
            RemakeAndRepaint();
        }

        private void OnEnable()
        {
            // Make sure the tools still work on project reload.
            _isOpen = HasOpenInstances<TrackToolsWindow>();

            if (_isOpen)
            {
                PrepareSelection();
                RegisterEvents(true);
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

        private void RegisterEvents(bool redraw)
        {
            SceneView.duringSceneGui += DrawHandles;
            Selection.selectionChanged += PrepareSelection;

            if (redraw)
            {
                RemakeAndRepaint();
            }
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

        public void CreateTrack(Vector3 attachPoint, Vector3 handlePosition)
        {
            switch (_currentPiece)
            {
                case TrackPiece.Straight:
                    SelectTrack(TrackToolsCreator.CreateStraight(TrackPrefab, _currentParent, attachPoint, handlePosition,
                        _length, _endGrade, true));
                    break;
                case TrackPiece.Curve:
                    SelectTrack(TrackToolsCreator.CreateCurve(TrackPrefab, _currentParent, attachPoint, handlePosition, _orientation,
                        _radius, _arc, _maxArcPerPoint, _endGrade, true));
                    break;
                case TrackPiece.Switch:
                    SelectTrack(TrackToolsCreator.CreateSwitch(LeftSwitch, RightSwitch, _currentParent, attachPoint, handlePosition,
                        _orientation, _connectingPoint, true).ThroughTrack);
                    break;
                case TrackPiece.Yard:
                    SelectTrack(TrackToolsCreator.CreateYard(LeftSwitch, RightSwitch, TrackPrefab, _currentParent, attachPoint, handlePosition,
                        _orientation, _trackDistance, _yardOptions, out _, true)[0].ThroughTrack);
                    break;
                case TrackPiece.Turntable:
                    SelectTrack(TrackToolsCreator.CreateTurntable(TurntablePrefab, TrackPrefab, _currentParent, attachPoint, handlePosition,
                        _turntableOptions, true, out _).Track);
                    break;
                case TrackPiece.Special:
                    CreateSpecial(attachPoint, handlePosition);
                    break;
                default:
                    throw new System.Exception("Invalid mode!");
            }
        }

        public void CreateSpecial(Vector3 attachPoint, Vector3 handlePosition)
        {
            switch (_currentSpecial)
            {
                case SpecialTrackPiece.Buffer:
                    TrackToolsCreator.CreateBuffer(BufferPrefab, _currentParent, attachPoint, handlePosition, true);
                    break;
                case SpecialTrackPiece.SwitchCurve:
                    SelectTrack(TrackToolsCreator.CreateSwitchCurve(LeftSwitch, RightSwitch, _currentParent, attachPoint, handlePosition,
                        _orientation, _connectingPoint, true));
                    break;
                case SpecialTrackPiece.Connect2:
                    CreateConnect2();
                    break;
                case SpecialTrackPiece.Crossover:
                    SelectTrack(TrackToolsCreator.CreateCrossover(LeftSwitch, RightSwitch, TrackPrefab, _currentParent, attachPoint, handlePosition,
                        _orientation, _trackDistance, _isTrailing, _switchDistance, true)[0].ThroughTrack);
                    break;
                case SpecialTrackPiece.ScissorsCrossover:
                    SelectTrack(TrackToolsCreator.CreateScissorsCrossover(LeftSwitch, RightSwitch, TrackPrefab, _currentParent, attachPoint, handlePosition,
                        _orientation, _trackDistance, _switchDistance, true)[3].ThroughTrack);
                    break;
                case SpecialTrackPiece.DoubleSlip:
                    SelectTrack(TrackToolsCreator.CreateDoubleSlip(LeftSwitch, RightSwitch, TrackPrefab, _currentParent, attachPoint, handlePosition,
                        _orientation, _crossAngle, true)[2].ThroughTrack);
                    break;
                default:
                    throw new System.Exception("Invalid mode!");
            }
        }

        private void CreateConnect2()
        {
            switch (_selectionType)
            {
                case SelectionType.Track:
                    BezierPoint p0 = _useHandle2Start ? _selectedTracks[0].Curve[0] : _selectedTracks[0].Curve.Last();
                    BezierPoint p1 = _useHandle2End ? _selectedTracks[1].Curve[0] : _selectedTracks[1].Curve.Last();

                    SelectTrack(TrackToolsCreator.CreateConnect2Point(TrackPrefab, _currentParent, p0, p1,
                        _useHandle2Start, _useHandle2End, _lengthMultiplier, true));

                    break;
                case SelectionType.BezierPoint:
                    SelectTrack(TrackToolsCreator.CreateConnect2Point(TrackPrefab, _currentParent, _selectedPoints[0], _selectedPoints[1],
                                _useHandle2Start, _useHandle2End, _lengthMultiplier, true));
                    break;
                case SelectionType.None:
                default:
                    break;
            }
        }

        public void DeleteTrack()
        {
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

            PrepareSelection();
        }

        #endregion

        #region OTHER

#if UNITY_EDITOR
        public void TryGetDefaultAssets()
        {
            string[] guids;

            if (TrackPrefab == null)
            {
                guids = AssetDatabase.FindAssets("Track", new[] { "Assets/Mapify/Prefabs/Trackage" });

                if (guids.Length > 0)
                {
                    TrackPrefab = AssetDatabase.LoadAssetAtPath<Track>(AssetDatabase.GUIDToAssetPath(guids[0]));
                }
            }

            if (BufferPrefab == null)
            {
                guids = AssetDatabase.FindAssets("Buffer Stop", new[] { "Assets/Mapify/Prefabs/Trackage" });

                if (guids.Length > 0)
                {
                    BufferPrefab = AssetDatabase.LoadAssetAtPath<BufferStop>(AssetDatabase.GUIDToAssetPath(guids[0]));
                }
            }

            if (LeftSwitch == null)
            {
                guids = AssetDatabase.FindAssets("Switch Left", new[] { "Assets/Mapify/Prefabs/Trackage" });

                if (guids.Length > 0)
                {
                    LeftSwitch = AssetDatabase.LoadAssetAtPath<Switch>(AssetDatabase.GUIDToAssetPath(guids[0]));
                }
            }

            if (RightSwitch == null)
            {
                guids = AssetDatabase.FindAssets("Switch Right", new[] { "Assets/Mapify/Prefabs/Trackage" });

                if (guids.Length > 0)
                {
                    RightSwitch = AssetDatabase.LoadAssetAtPath<Switch>(AssetDatabase.GUIDToAssetPath(guids[0]));
                }
            }

            if (TurntablePrefab == null)
            {
                guids = AssetDatabase.FindAssets("Turntable", new[] { "Assets/Mapify/Prefabs/Trackage" });

                for (int i = 0; i < guids.Length; i++)
                {
                    var turn = AssetDatabase.LoadAssetAtPath<Turntable>(AssetDatabase.GUIDToAssetPath(guids[i]));

                    if (turn != null)
                    {
                        TurntablePrefab = turn;
                        break;
                    }
                }
            }
        }
#endif

        public void ResetCreationSettings(bool all)
        {
            _endGrade = 0.0f;

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
            SelectObject(t.gameObject);
        }

        private void SelectObject(GameObject go)
        {
            Selection.activeGameObject = go;
            RemakeAndRepaint();
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
            CreatePreviews();
            SceneView.RepaintAll();
        }

        private void CreatePreviews()
        {
            ClearPreviews();
            DoNullCheck();

            if (!_isOpen)
            {
                return;
            }

            Vector3 pos = _currentParent ? _currentParent.position : Vector3.zero;
            Vector3 forward = _currentParent ? _currentParent.forward : Vector3.forward;

            AttachPoint next;
            AttachPoint prev;

            bool createFront = false;
            bool createBack = false;
            bool createNew = _drawNewPreview;

            switch (_selectionType)
            {
                case SelectionType.Track:
                    next = new AttachPoint(CurrentTrack.Curve.Last().position, CurrentTrack.Curve.Last().globalHandle1);
                    prev = new AttachPoint(CurrentTrack.Curve[0].position, CurrentTrack.Curve[0].globalHandle2);

                    createFront = CurrentTrack && CheckGrade(CurrentTrack.GetGradeAtEnd());
                    createBack = CurrentTrack && CheckGrade(CurrentTrack.GetGradeAtStart());
                    break;
                case SelectionType.BezierPoint:
                    next = new AttachPoint(CurrentPoint.position, CurrentPoint.globalHandle1);
                    prev = new AttachPoint(CurrentPoint.position, CurrentPoint.globalHandle2);

                    createFront = CurrentPoint && !Mathf.Approximately(CurrentPoint.handle1.sqrMagnitude, 0) &&
                        CheckGrade(CurrentPoint.GetGradeForwards());
                    createBack = CurrentPoint && !Mathf.Approximately(CurrentPoint.handle2.sqrMagnitude, 0) &&
                        CheckGrade(CurrentPoint.GetGradeBackwards());
                    break;
                default:
                    next = new AttachPoint(Vector3.zero, Vector3.zero);
                    prev = new AttachPoint(Vector3.zero, Vector3.zero);
                    break;
            }

            switch (_currentPiece)
            {
                case TrackPiece.Straight:
                    if (createFront)
                    {
                        _forwardLines = new Vector3[][] { TrackToolsCreator.Previews.PreviewStraight(
                            next.Position, next.Handle,
                            _length, _endGrade, out _forwardPoints, _sampleCount) };
                    }
                    if (createBack)
                    {
                        _backwardLines = new Vector3[][] { TrackToolsCreator.Previews.PreviewStraight(
                            prev.Position, prev.Handle,
                            _length, _endGrade, out _backwardPoints, _sampleCount) };
                    }
                    if (createNew)
                    {
                        _newLines = new Vector3[][] { TrackToolsCreator.Previews.PreviewStraight(pos, pos - forward,
                            _length, _endGrade, out _newPoints, _sampleCount) };
                    }
                    break;
                case TrackPiece.Curve:
                    if (createFront)
                    {
                        _forwardLines = new Vector3[][] { TrackToolsCreator.Previews.PreviewCurve(
                            next.Position, next.Handle,
                            _orientation, _radius, _arc, _maxArcPerPoint, _endGrade, out _forwardPoints, _sampleCount) };
                    }
                    if (createBack)
                    {
                        _backwardLines = new Vector3[][] { TrackToolsCreator.Previews.PreviewCurve(
                            prev.Position, prev.Handle,
                            _orientation, _radius, _arc, _maxArcPerPoint, _endGrade, out _backwardPoints, _sampleCount) };
                    }
                    if (createNew)
                    {
                        _newLines = new Vector3[][] { TrackToolsCreator.Previews.PreviewCurve(pos, pos - forward,
                            _orientation, _radius, _arc, _maxArcPerPoint, _endGrade, out _newPoints, _sampleCount) };
                    }
                    break;
                case TrackPiece.Switch:
                    if (LeftSwitch && RightSwitch)
                    {
                        if (createFront)
                        {
                            _forwardPoints = new Vector3[] { next.Position };

                            _forwardLines = TrackToolsCreator.Previews.PreviewSwitch(GetCurrentSwitchPrefab(),
                                next.Position, next.Handle,
                                _connectingPoint, _sampleCount);
                        }
                        if (createBack)
                        {
                            _backwardPoints = new Vector3[] { prev.Position };

                            _backwardLines = TrackToolsCreator.Previews.PreviewSwitch(GetCurrentSwitchPrefab(),
                                prev.Position, prev.Handle,
                                _connectingPoint, _sampleCount);
                        }
                        if (createNew)
                        {
                            _newPoints = new Vector3[] { pos };

                            forward.y = 0;
                            _newLines = TrackToolsCreator.Previews.PreviewSwitch(GetCurrentSwitchPrefab(),
                                pos, pos - forward, _connectingPoint, _sampleCount);
                        }
                    }
                    break;
                case TrackPiece.Yard:
                    if (LeftSwitch && RightSwitch)
                    {
                        if (createFront)
                        {
                            _forwardLines = TrackToolsCreator.Previews.PreviewYard(LeftSwitch, RightSwitch,
                                next.Position, next.Handle,
                                _orientation, _trackDistance, _yardOptions, _sampleCount);
                        }
                        if (createBack)
                        {
                            _backwardLines = TrackToolsCreator.Previews.PreviewYard(LeftSwitch, RightSwitch,
                                prev.Position, prev.Handle,
                                _orientation, _trackDistance, _yardOptions, _sampleCount);
                        }
                        if (createNew)
                        {
                            forward.y = 0;
                            _newLines = TrackToolsCreator.Previews.PreviewYard(LeftSwitch, RightSwitch,
                                pos, pos - forward, _orientation, _trackDistance, _yardOptions, _sampleCount);
                        }
                    }
                    break;
                case TrackPiece.Turntable:
                    if (createFront)
                    {
                        _forwardLines = TrackToolsCreator.Previews.PreviewTurntable(
                            next.Position, next.Handle, _turntableOptions, _sampleCount);
                    }
                    if (createBack)
                    {
                        _backwardLines = TrackToolsCreator.Previews.PreviewTurntable(
                            prev.Position, prev.Handle, _turntableOptions, _sampleCount);
                    }
                    if (createNew)
                    {
                        forward.y = 0;
                        _newLines = TrackToolsCreator.Previews.PreviewTurntable(pos, pos - forward, _turntableOptions, _sampleCount);
                    }
                    break;
                case TrackPiece.Special:
                    SpecialPreviews(createFront, createBack, createNew, pos, forward, next, prev);
                    break;
                default:
                    break;
            }
        }

        private void SpecialPreviews(bool createFront, bool createBack, bool createNew, Vector3 pos, Vector3 forward,
            AttachPoint next, AttachPoint prev)
        {
            switch (_currentSpecial)
            {
                case SpecialTrackPiece.Buffer:
                    if (createFront)
                    {
                        _forwardPoints = new Vector3[] { next.Position };
                    }
                    if (createBack)
                    {
                        _backwardPoints = new Vector3[] { prev.Position };
                    }
                    if (createNew)
                    {
                        _newPoints = new Vector3[] { pos };
                    }
                    break;
                case SpecialTrackPiece.SwitchCurve:
                    if (LeftSwitch && RightSwitch)
                    {
                        if (createFront)
                        {
                            _forwardPoints = new Vector3[] { next.Position };

                            _forwardLines = new Vector3[1][];
                            System.Array.Copy(TrackToolsCreator.Previews.PreviewSwitch(GetCurrentSwitchPrefab(),
                                next.Position, next.Handle,
                                _connectingPoint, _sampleCount), 1, _forwardLines, 0, 1);
                        }
                        if (createBack)
                        {
                            _backwardPoints = new Vector3[] { prev.Position };

                            _backwardLines = new Vector3[1][];
                            System.Array.Copy(TrackToolsCreator.Previews.PreviewSwitch(GetCurrentSwitchPrefab(),
                                prev.Position, prev.Handle,
                                _connectingPoint, _sampleCount), 1, _backwardLines, 0, 1);
                        }
                        if (createNew)
                        {
                            _newPoints = new Vector3[] { pos };

                            _newLines = new Vector3[1][];
                            forward.y = 0;
                            System.Array.Copy(TrackToolsCreator.Previews.PreviewSwitch(GetCurrentSwitchPrefab(),
                                pos, pos - forward, SwitchPoint.Joint, _sampleCount), 1, _newLines, 0, 1);
                        }
                    }
                    break;
                case SpecialTrackPiece.Connect2:
                    {
                        BezierPoint p0 = null;
                        BezierPoint p1 = null;

                        switch (_selectionType)
                        {
                            case SelectionType.Track:
                                p0 = _useHandle2Start ? _selectedTracks[0].Curve[0] : _selectedTracks[0].Curve.Last();
                                if (_selectedTracks.Length > 1)
                                {
                                    p1 = _useHandle2End ? _selectedTracks[1].Curve[0] : _selectedTracks[1].Curve.Last();
                                }
                                break;
                            case SelectionType.BezierPoint:
                                p0 = _selectedPoints[0];
                                if (_selectedPoints.Length > 1)
                                {
                                    p1 = _selectedPoints[1];
                                }
                                break;
                            case SelectionType.None:
                            default:
                                break;
                        }

                        if (p0)
                        {
                            _forwardPoints = new Vector3[] { p0.position,
                                MathHelper.MirrorAround(_useHandle2Start ? p0.globalHandle2 :p0.globalHandle1,
                                p0.position) };

                            _forwardLines = new Vector3[][] { _forwardPoints };
                        }

                        if (p0 && p1)
                        {
                            _backwardPoints = new Vector3[] { p1.position,
                                MathHelper.MirrorAround(_useHandle2End ? p1.globalHandle2 : p1.globalHandle1,
                                p1.position) };

                            _backwardLines = new Vector3[][] { _backwardPoints };

                            _newLines = new Vector3[][] { TrackToolsCreator.Previews.PreviewConnect2(
                                p0.position,
                                _useHandle2Start ? p0.globalHandle2 : p0.globalHandle1,
                                p1.position,
                                _useHandle2End ? p1.globalHandle2 : p1.globalHandle1,
                                _lengthMultiplier,
                                _sampleCount) };
                        }
                    }
                    break;
                case SpecialTrackPiece.Crossover:
                    if (LeftSwitch && RightSwitch)
                    {
                        if (createFront)
                        {
                            _forwardPoints = new Vector3[] { next.Position };

                            _forwardLines = TrackToolsCreator.Previews.PreviewCrossover(GetCurrentSwitchPrefab(),
                                next.Position, next.Handle, _orientation, _trackDistance, _isTrailing,
                                _switchDistance, _sampleCount);
                        }
                        if (createBack)
                        {
                            _backwardPoints = new Vector3[] { prev.Position };

                            _backwardLines = TrackToolsCreator.Previews.PreviewCrossover(GetCurrentSwitchPrefab(),
                                prev.Position, prev.Handle, _orientation, _trackDistance, _isTrailing,
                                _switchDistance, _sampleCount);
                        }
                        if (createNew)
                        {
                            _newPoints = new Vector3[] { pos };

                            forward.y = 0;
                            _newLines = TrackToolsCreator.Previews.PreviewCrossover(GetCurrentSwitchPrefab(), pos, pos - forward,
                                _orientation, _trackDistance, _isTrailing, _switchDistance, _sampleCount);
                        }
                    }
                    break;
                case SpecialTrackPiece.ScissorsCrossover:
                    if (LeftSwitch && RightSwitch)
                    {
                        if (createFront)
                        {
                            _forwardPoints = new Vector3[] { next.Position };

                            _forwardLines = TrackToolsCreator.Previews.PreviewScissorsCrossover(LeftSwitch, RightSwitch,
                                next.Position, next.Handle,
                                _orientation, _trackDistance, _switchDistance, _sampleCount);
                        }
                        if (createBack)
                        {
                            _backwardPoints = new Vector3[] { prev.Position };

                            _backwardLines = TrackToolsCreator.Previews.PreviewScissorsCrossover(LeftSwitch, RightSwitch,
                                prev.Position, prev.Handle,
                                _orientation, _trackDistance, _switchDistance, _sampleCount);
                        }
                        if (createNew)
                        {
                            _newPoints = new Vector3[] { pos };

                            forward.y = 0;
                            _newLines = TrackToolsCreator.Previews.PreviewScissorsCrossover(LeftSwitch, RightSwitch, pos, pos - forward,
                                _orientation, _trackDistance, _switchDistance, _sampleCount);
                        }
                    }
                    break;
                case SpecialTrackPiece.DoubleSlip:
                    if (LeftSwitch && RightSwitch)
                    {
                        if (createFront)
                        {
                            _forwardPoints = new Vector3[] { next.Position };

                            _forwardLines = TrackToolsCreator.Previews.PreviewDoubleSlip(LeftSwitch, RightSwitch,
                                next.Position, next.Handle,
                                _orientation, _crossAngle, _sampleCount);
                        }
                        if (createBack)
                        {
                            _backwardPoints = new Vector3[] { prev.Position };

                            _backwardLines = TrackToolsCreator.Previews.PreviewDoubleSlip(LeftSwitch, RightSwitch,
                                prev.Position, prev.Handle,
                                _orientation, _crossAngle, _sampleCount);
                        }
                        if (createNew)
                        {
                            _newPoints = new Vector3[] { pos };

                            forward.y = 0;
                            _newLines = TrackToolsCreator.Previews.PreviewDoubleSlip(LeftSwitch, RightSwitch, pos, pos - forward,
                                _orientation, _crossAngle, _sampleCount);
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        private Switch GetCurrentSwitchPrefab()
        {
            return _isLeft ? LeftSwitch : RightSwitch;
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

        private void DrawHandles(SceneView scene)
        {
            // Only draw handles for track creation if the creation foldout is active.
            if (_showCreation)
            {
                DrawCreationPreviews();
            }

            if (_showEditing)
            {
                DrawEditingPreviews();
            }

            // Extra curve drawing.
            if (CurrentTrack)
            {
                DrawExtraPreviews();
            }
        }

        private void DrawCreationPreviews()
        {
            using (new Handles.DrawingScope(_backwardColour))
            {
                for (int i = 0; i < _backwardPoints.Length; i++)
                {
                    // TODO: use a disc instead of this so it is not selectable.
                    Handles.FreeRotateHandle(Quaternion.identity, _backwardPoints[i],
                        HandleUtility.GetHandleSize(_backwardPoints[i]) * (i == 0 ? 0.15f : 0.05f));
                }

                for (int i = 0; i < _backwardLines.Length; i++)
                {
                    Handles.DrawPolyLine(_backwardLines[i]);
                }
            }

            using (new Handles.DrawingScope(_forwardColour))
            {
                for (int i = 0; i < _forwardPoints.Length; i++)
                {
                    Handles.FreeRotateHandle(Quaternion.identity, _forwardPoints[i],
                        HandleUtility.GetHandleSize(_forwardPoints[i]) * (i == 0 ? 0.15f : 0.05f));
                }

                for (int i = 0; i < _forwardLines.Length; i++)
                {
                    Handles.DrawPolyLine(_forwardLines[i]);
                }
            }

            using (new Handles.DrawingScope(_newColour))
            {
                for (int i = 0; i < _newPoints.Length; i++)
                {
                    Handles.FreeRotateHandle(Quaternion.identity, _newPoints[i],
                        HandleUtility.GetHandleSize(_newPoints[i]) * (i == 0 ? 0.15f : 0.05f));
                }

                for (int i = 0; i < _newLines.Length; i++)
                {
                    Handles.DrawPolyLine(_newLines[i]);

                    //Vector3 p = _newLines[i][0];
                    //Handles.FreeRotateHandle(Quaternion.identity, p,
                    //    HandleUtility.GetHandleSize(p) * 0.10f);
                    //p = _newLines[i][_newLines[i].Length - 1];
                    //Handles.FreeRotateHandle(Quaternion.identity, p,
                    //    HandleUtility.GetHandleSize(p) * 0.05f);
                }
            }
        }

        private void DrawEditingPreviews()
        {

        }

        private void DrawExtraPreviews()
        {
            // If there's a height change, draw the same curve but completely level.
            if (CurrentTrack.Curve[0].position.y != CurrentTrack.Curve.Last().position.y)
            {
                float y = CurrentTrack.Curve[0].position.y;
                Vector3 p0, p1, p2, p3;

                using (new Handles.DrawingScope(CurrentTrack.Curve.drawColor.Negative()))
                {
                    p0 = CurrentTrack.Curve[0].position;
                    p1 = CurrentTrack.Curve[0].globalHandle2;

                    Handles.Label(p0 + Vector3.up * HandleUtility.GetHandleSize(p0),
                        $"{MathHelper.GetGrade(p0, p1) * 100.0f:F2}%");

                    for (int i = 1; i < CurrentTrack.Curve.pointCount; i++)
                    {
                        p0 = CurrentTrack.Curve[i - 1].position;
                        p1 = CurrentTrack.Curve[i - 1].globalHandle2;
                        p2 = CurrentTrack.Curve[i].globalHandle1;
                        p3 = CurrentTrack.Curve[i].position;

                        Handles.Label(p3 + Vector3.up * HandleUtility.GetHandleSize(p3),
                            $"{MathHelper.GetGrade(p2, p3) * 100.0f:F2}%");

                        p0.y = y;
                        p1.y = y;
                        p2.y = y;
                        p3.y = y;

                        EditorHelper.DrawBezier(p0, p1, p2, p3, _sampleCount);
                        Handles.DrawLine(p3, CurrentTrack.Curve[i].position);
                    }
                }
            }
        }

        private void ClearPreviews()
        {
            _forwardPoints = new Vector3[0];
            _newPoints = new Vector3[0];
            _backwardPoints = new Vector3[0];
            _forwardLines = new Vector3[0][];
            _newLines = new Vector3[0][];
            _backwardLines = new Vector3[0][];

            TrackToolsCreator.Previews.CachedYard = null;
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

        #endregion

        #region STRUCTS

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
        }

        #endregion
    }
}
#endif
