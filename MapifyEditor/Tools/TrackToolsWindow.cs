using Mapify.Editor.Tools.OptionData;
using Mapify.Editor.Utils;
using UnityEditor;
using UnityEngine;
using static Mapify.Editor.Tools.ToolEnums;

#if UNITY_EDITOR
namespace Mapify.Editor.Tools
{
    public class TrackToolsWindow : EditorWindow
    {
        [MenuItem("Mapify/Track Tools")]
        public static void ShowWindow()
        {
            var window = GetWindow<TrackToolsWindow>();
            window.Show();
            window.titleContent = new GUIContent("Track Tools");
            window.autoRepaintOnSceneChange = true;
        }

        #region FIELDS

        // Selection.
        private bool _showSelection = true;
        private SelectionType _selectionType = SelectionType.None;
        private Track[] _selectedTracks = new Track[0];
        private BezierPoint[] _selectedPoints = new BezierPoint[0];
        private bool _showTracks = true;
        private bool _showPoints = true;

        // Track creation.
        private bool _showCreation = true;
        private CreationMode _currentMode = CreationMode.Straight;
        private Transform _currentParent;
        private TrackOrientation _orientation;
        private float _endGrade = 0.0f;
        private float _trackDistance = 4.5f;

        // Straight.
        private float _length = 100.0f;

        // Curves.
        private float _radius = 500.0f;
        private float _arc = 45.0f;
        private float _maxArcPerPoint = 22.5f;
        private bool _changeArc = false;

        // Switches.
        private SwitchPoint _connectingPoint = SwitchPoint.Joint;

        // Yards.
        private YardOptions _yardOptions = YardOptions.DefaultOptions;
        private bool _showYardCache = false;

        // Turntable.
        private TurntableOptions _turntableOptions = TurntableOptions.DefaultOptions;

        // Special.
        private SpecialTrack _currentSpecial = SpecialTrack.Buffer;
        private bool _useHandle2Start = false;
        private bool _useHandle2End = false;
        private float _lengthMultiplier = 1.0f;
        private bool _isTrailing = false;
        private float _switchDistance = 6.0f;
        private float _crossAngle = 20.0f;

        // Track editing.
        private bool _showEditing = false;
        private EditingMode _editingMode = EditingMode.Merge;

        // Track prefabs.
        private bool _showPrefabs = false;
        public Track TrackPrefab;
        public BufferStop BufferPrefab;
        public Switch LeftSwitch;
        public Switch RightSwitch;
        public Turntable TurntablePrefab;

        // Settings.
        private bool _showSettings = false;
        private Color _forwardColour = Color.cyan;
        private Color _backwardColour = Color.red;
        private Color _newColour = Color.green;
        private int _sampleCount = 8;

        // Contents for the tool selection.
        private readonly GUIContent[] _modeContents = {
            new GUIContent("Straight", "Straight tracks with a custom length"),
            new GUIContent("Curve", "Curves that approximate a circular arc"),
            new GUIContent("Switch", "Track switches"),
            new GUIContent("Yard", "Yards and sidings"),
            new GUIContent("Turntable", "Turntables"),
            new GUIContent("Special", "Includes multiple track pieces including:\n- Buffers\n- Intersections/Crossovers\nAnd more!")
        };

        // Special track selection.
        private readonly GUIContent[] _specialContents = {
            new GUIContent("Buffer", "A buffer stop at the end of a track"),
            new GUIContent("Switch curve", "The curve used by switches"),
            new GUIContent("Connect 2", "Connect 2 bezier points smoothly"),
            new GUIContent("Crossover", "A pair of switches that allows changing between 2 parallel tracks"),
            new GUIContent("Scissors crossover", "2 crossovers at the same time"),
            new GUIContent("Double slip", "A switch arrangement often found near stations")
        };

        // Editing mode.
        private readonly GUIContent[] _editingModeContents = {
            new GUIContent("Merge", "Merges multiple tracks into a one"),
        };

        // Window.
        private float _scrollMain = 0;

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

        private void OnFocus()
        {
            SceneView.duringSceneGui += DrawHandles;
            Selection.selectionChanged += PrepareSelection;
            RemakeAndRepaint();
        }

        private void OnLostFocus()
        {
            // Was supposed to stop drawing after losing focus but in this case it also
            // stops when changing selections. Actual visible functions only exist in
            // later versions of unity.
            //SceneView.duringSceneGui -= DrawHandles;
        }

        private void OnDestroy()
        {
            SceneView.duringSceneGui -= DrawHandles;
            Selection.selectionChanged -= PrepareSelection;
        }

        private void OnSelectionChange()
        {
            RemakeAndRepaint();
        }

        private void DoNullCheck()
        {
            // Null checks in case something changes unexpectedly
            if ((_selectedTracks.Length > 0 && !CurrentTrack) ||
                (_selectedPoints.Length > 0 && !CurrentPoint))
            {
                PrepareSelection();
            }
        }

        // Called when editor selection changes.
        private void PrepareSelection()
        {
            GameObject go = Selection.activeGameObject;

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

        private void DrawSelectionFoldout()
        {
            // What we're working with currently.
            GUI.backgroundColor *= 1.1f;

            _showSelection = EditorGUILayout.BeginFoldoutHeaderGroup(_showSelection,
                new GUIContent("Selection", "Properties of the current selection"),
                null, null);

            GUI.backgroundColor = Color.white;

            if (_showSelection)
            {
                EditorGUI.indentLevel++;

                switch (_selectionType)
                {
                    case SelectionType.Track:
                        DrawTrackSelection();
                        break;
                    case SelectionType.BezierPoint:
                        DrawPointSelection();
                        break;
                    default:
                        EditorGUILayout.Space();
                        EditorGUILayout.HelpBox("No compatible objects selected! These tools work with the following:\n" +
                            "\u2022 Tracks\n\u2022 BezierPoints", MessageType.Warning);
                        EditorGUILayout.Space();
                        break;
                }

                EditorGUILayout.Space();
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawTrackSelection()
        {
            EditorGUILayout.ObjectField(
                new GUIContent($"Current track"),
                CurrentTrack, typeof(Track), true);

            EditorGUILayout.LabelField("Total selected", _selectedTracks.Length.ToString());

            bool isSwitch = CurrentTrack.IsSwitch;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Part of switch", isSwitch.ToString());

            // If it's a switch add a button to swap between the 2 tracks.
            if (isSwitch)
            {
                if (GUILayout.Button(new GUIContent("Swap tracks", "Swaps the selected track between the through and diverging tracks")))
                {
                    Switch s = CurrentTrack.GetComponentInParent<Switch>();

                    if (CurrentTrack == s.ThroughTrack)
                    {
                        Selection.activeGameObject = s.DivergingTrack.gameObject;
                        SelectTrack(s.DivergingTrack);
                    }
                    else
                    {
                        Selection.activeGameObject = s.ThroughTrack.gameObject;
                        SelectTrack(s.ThroughTrack);
                    }
                }
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Part of turntable", CurrentTrack.IsTurntable.ToString());

            EditorGUILayout.LabelField(new GUIContent("Length", "Length of the track"),
                new GUIContent($"{CurrentTrack.Curve.length:F3}m"));
            EditorGUILayout.LabelField(new GUIContent("Horizontal length", "Length of the track with no vertical changes"),
                new GUIContent($"{CurrentTrack.GetHorizontalLength():F3}m"));

            EditorGUILayout.LabelField("At start");
            EditorGUI.indentLevel++;

            EditorGUILayout.LabelField(new GUIContent("Grade", "Grade at the start of the track"),
                new GUIContent($"{CurrentTrack.GetGradeAtStart() * 100.0f:F2}%"));
            EditorGUILayout.LabelField(new GUIContent("North angle", "Angle in relation to the North at start of the track"),
                new GUIContent($"{MathHelper.AngleToNorth(CurrentTrack.Curve[0].globalHandle2 - CurrentTrack.Curve[0].position):F2}°"));

            EditorGUI.indentLevel--;
            EditorGUILayout.LabelField("At end");
            EditorGUI.indentLevel++;

            EditorGUILayout.LabelField(new GUIContent("Grade", "Grade at the end of the track"),
                new GUIContent($"{CurrentTrack.GetGradeAtEnd() * 100.0f:F2}%"));
            EditorGUILayout.LabelField(new GUIContent("North angle", "Angle in relation to the North at end of the track"),
                new GUIContent($"{MathHelper.AngleToNorth(CurrentTrack.Curve.Last().position - CurrentTrack.Curve.Last().globalHandle1):F2}°"));

            EditorGUI.indentLevel--;

            EditorGUILayout.LabelField(new GUIContent("Height difference", "Total change in height along the track"),
                new GUIContent($"{CurrentTrack.GetHeightChange():F3}m"));
            EditorGUILayout.LabelField(new GUIContent("Average grade", "Average grade of the track"),
                new GUIContent($"{CurrentTrack.GetAverageGrade() * 100.0f:F2}%"));
        }

        private void DrawPointSelection()
        {
            EditorGUILayout.ObjectField(
                new GUIContent("Current point"),
                CurrentPoint, typeof(BezierPoint), true);

            EditorGUILayout.LabelField("Handle 1");
            EditorGUI.indentLevel++;

            if (CurrentPoint.handle1.sqrMagnitude > 0)
            {
                EditorGUILayout.LabelField(new GUIContent("Grade", "Grade through handle 1"),
                    new GUIContent($"{MathHelper.GetGrade(CurrentPoint.position, CurrentPoint.globalHandle1) * 100.0f:F2}%"));
                EditorGUILayout.LabelField(new GUIContent("North angle", "Angle in relation to the North through handle 1"),
                    new GUIContent($"{MathHelper.AngleToNorth(CurrentPoint.position - CurrentPoint.globalHandle1):F2}°"));
            }
            else
            {
                EditorGUILayout.LabelField("Handle has 0 length!");
            }
            EditorGUI.indentLevel--;

            EditorGUILayout.LabelField("Handle 2");
            EditorGUI.indentLevel++;

            if (CurrentPoint.handle2.sqrMagnitude > 0)
            {
                EditorGUILayout.LabelField(new GUIContent("Grade", "Grade through handle 2"),
                    new GUIContent($"{MathHelper.GetGrade(CurrentPoint.position, CurrentPoint.globalHandle2) * 100.0f:F2}%"));
                EditorGUILayout.LabelField(new GUIContent("North angle", "Angle in relation to the North through handle 2"),
                    new GUIContent($"{MathHelper.AngleToNorth(CurrentPoint.position - CurrentPoint.globalHandle2):F2}°"));
            }
            else
            {
                EditorGUILayout.LabelField("Handle has 0 length!");
            }
            EditorGUI.indentLevel--;

            _showPoints = EditorHelper.MultipleSelectionFoldout("Selected points", "BezierPoint", _showPoints, _selectedPoints);
        }

        private void DrawCreationFoldout()
        {
            GUI.backgroundColor *= 1.1f;

            _showCreation = EditorGUILayout.BeginFoldoutHeaderGroup(_showCreation,
                new GUIContent("Creation", "Menu where creation happens"),
                null, CreationFoldoutContextMenu);

            GUI.backgroundColor = Color.white;

            if (_showCreation)
            {
                EditorGUILayout.Space();
                EditorGUI.indentLevel++;

                _currentParent = (Transform)EditorGUILayout.ObjectField(
                    new GUIContent("Track parent", "The parent transform for new tracks"),
                    _currentParent, typeof(Transform), true);
                EditorGUILayout.Space();

                // Select the current editing mode.
                GUI.backgroundColor *= 0.8f;
                _currentMode = (CreationMode)GUILayout.SelectionGrid((int)_currentMode, _modeContents, 6, EditorStyles.miniButtonMid);
                GUI.backgroundColor = Color.white;
                EditorGUILayout.Space();

                switch (_currentMode)
                {
                    case CreationMode.Straight:
                        DrawStraightOptions();
                        break;
                    case CreationMode.Curve:
                        DrawCurveOptions();
                        break;
                    case CreationMode.Switch:
                        DrawSwitchOptions();
                        break;
                    case CreationMode.Yard:
                        DrawYardOptions();
                        break;
                    case CreationMode.Turntable:
                        DrawTurntableOptions();
                        break;
                    case CreationMode.Special:
                        DrawSpecialOptions();
                        break;
                    default:
                        EditorGUILayout.HelpBox("Not implemented yet!", MessageType.Warning);
                        break;
                }

                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
                DoNullCheck();
                DrawCreationButtons();
                EditorGUILayout.Space();

                // Only have one of the 2 open.
                _showEditing = false;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        #region CREATION OPTIONS

        private void DrawStraightOptions()
        {
            if (!Require(TrackPrefab, "Track prefab"))
            {
                return;
            }

            _length = EditorGUILayout.FloatField(
                    new GUIContent("Length", "Length of the next track section"),
                    _length);
            _endGrade = EditorGUILayout.FloatField(
                new GUIContent("End grade", "How steep should the track's other end be"),
                _endGrade * 100.0f) / 100.0f;

            EditorGUILayout.Space();

            // Extra info.

            if (CurrentTrack)
            {
                EditorGUILayout.LabelField(new GUIContent("Height dif back",
                    "Height difference backwards"),
                    new GUIContent($"{TrackToolsHelper.CalculateHeightDifference(CurrentTrack.GetGradeAtStart(), _endGrade, _length):F3}m"));
                EditorGUILayout.LabelField(new GUIContent("Height dif front",
                    "Height difference forwards"),
                    new GUIContent($"{TrackToolsHelper.CalculateHeightDifference(CurrentTrack.GetGradeAtEnd(), _endGrade, _length):F3}m"));
            }

            EditorGUILayout.LabelField(new GUIContent("Height dif new",
                "Height difference for new tracks"),
                new GUIContent($"{TrackToolsHelper.CalculateHeightDifference(0, _endGrade, _length):F3}m"));
        }

        private void DrawCurveOptions()
        {
            if (!Require(TrackPrefab, "Track prefab"))
            {
                return;
            }

            DrawOrientationGUI("Which side the curve turns to");

            EditorGUILayout.BeginHorizontal();

            _radius = EditorGUILayout.FloatField(new GUIContent("Radius", "Radius of the curve"),
                _radius);

            if (GUILayout.Button(new GUIContent("Use switch radius", "Sets the radius to the one of switch curves"),
                GUILayout.MaxWidth(140)))
            {
                if (LeftSwitch)
                {
                    _radius = TrackToolsHelper.CalculateSwitchRadius(LeftSwitch);
                }
                else if (RightSwitch)
                {
                    _radius = TrackToolsHelper.CalculateSwitchRadius(RightSwitch);
                }
                else
                {
                    _radius = TrackToolsHelper.DefaultSwitchRadius;
                }
            }

            EditorGUILayout.EndHorizontal();

            _arc = EditorGUILayout.Slider(new GUIContent("Arc", "Angle of the curve"),
                _arc, 0.0f, 180.0f);
            _maxArcPerPoint = EditorGUILayout.Slider(new GUIContent("Max arc per point",
                "How big an arc can be before the curve is split."),
                _maxArcPerPoint, 0.0f, 90.0f);
            _endGrade = EditorGUILayout.FloatField(
                new GUIContent("End grade", "How steep should the track's other end be"),
                _endGrade * 100.0f) / 100.0f;

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            // Check if length was manually set.
            EditorGUI.BeginChangeCheck();
            float length = EditorGUILayout.FloatField(new GUIContent("Approx. length",
                "Approximated total length of the curve"),
                _radius * _arc * Mathf.Deg2Rad);
            bool changed = EditorGUI.EndChangeCheck();

            _changeArc = EditorGUILayout.ToggleLeft(new GUIContent("Change arc",
                "Change the arc of the curve instead of the radius to match the length"),
                _changeArc, GUILayout.MaxWidth(100));

            if (changed)
            {
                if (_changeArc)
                {
                    _arc = (length / _radius) * Mathf.Rad2Deg;
                }
                else
                {
                    _radius = length / (_arc * Mathf.Deg2Rad);
                }
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField(new GUIContent("Speed limit", "Estimated speed limit (shown on signs / closer estimate)"),
                new GUIContent($"{TrackToolsHelper.GetMaxSpeedForRadiusGame(_radius)}/{TrackToolsHelper.GetMaxSpeedForRadius(_radius):F1}km/h"));
        }

        private void DrawSwitchOptions()
        {
            if (!Require(LeftSwitch, "Left switch prefab") ||
                !Require(RightSwitch, "Right switch prefab"))
            {
                return;
            }

            DrawOrientationGUI("Which side the diverging track turns to");
            _connectingPoint = (SwitchPoint)EditorGUILayout.EnumPopup(new GUIContent("Connecting point",
                "Which of the 3 switch points should connect to the current track"),
                _connectingPoint);
        }

        private void DrawYardOptions()
        {
            if (!Require(TrackPrefab, "Track prefab") ||
                !Require(LeftSwitch, "Left switch prefab") ||
                !Require(RightSwitch, "Right switch prefab"))
            {
                return;
            }

            DrawOrientationGUI("Which side the first switch should diverge to");
            _trackDistance = EditorGUILayout.FloatField(new GUIContent("Track distance",
                "The distance between parallel tracks"),
                _trackDistance);
            _yardOptions.TracksMainSide = EditorGUILayout.IntField(new GUIContent("Tracks to main side",
                "Number of tracks to the side defined by the orientation"),
                _yardOptions.TracksMainSide);
            _yardOptions.TracksOtherSide = EditorGUILayout.IntField(new GUIContent("Tracks to other side",
                "Number of tracks to the side opposite to the orientation"),
                _yardOptions.TracksOtherSide);
            _yardOptions.AlternateSides = EditorGUILayout.Toggle(new GUIContent("Alternate sides",
                "If true, the switches at either end will turn to different sides of the yard, if false they will " +
                "instead face the same side"),
                _yardOptions.AlternateSides);
            _yardOptions.MinimumLength = EditorGUILayout.FloatField(new GUIContent("Minimum siding length",
                "The minimum length of the straigth part of a siding of this yard"),
                _yardOptions.MinimumLength);
            _yardOptions.StationId = EditorGUILayout.TextField(new GUIContent("Station ID",
                "ID of the station all the yard tracks belong to"),
                _yardOptions.StationId);
            _yardOptions.YardId = EditorHelper.CharField(new GUIContent("Yard ID",
                "ID of the yard in the station (single character)"),
                _yardOptions.YardId);
            _yardOptions.StartTrackId = (byte)EditorGUILayout.IntField(new GUIContent("Track ID",
                "Starting number of the track"),
                _yardOptions.StartTrackId);

            EditorGUILayout.Space();

            // Extra info.
            int totalTracks = _yardOptions.TracksMainSide + _yardOptions.TracksOtherSide;
            EditorGUILayout.LabelField("Total tracks", $"{totalTracks + 1}");
            EditorGUILayout.LabelField("Track numbers", $"{_yardOptions.StartTrackId} to {_yardOptions.StartTrackId + totalTracks}");

            if (_yardOptions.StartTrackId + totalTracks > 99)
            {
                EditorGUILayout.HelpBox("Track number cannot exceed 99.", MessageType.Error);
            }
            _showYardCache = EditorGUILayout.Foldout(_showYardCache, new GUIContent("Yard sizes"));

            if (!_showYardCache)
            {
                return;
            }

            EditorGUI.indentLevel++;

            if (TrackToolsCreator.Previews.CachedYard.HasValue)
            {

                EditorGUILayout.LabelField(new GUIContent("End to end length", "The total, end to end length of the yard"),
                    new GUIContent($"{TrackToolsCreator.Previews.CachedYard.Value.TotalLength}m"));
                EditorGUILayout.LabelField(new GUIContent("Width", "The width of the yard, including loading gauge."),
                    new GUIContent($"{totalTracks * _trackDistance + TrackToolsCreator.Previews.CachedYard.Value.LoadingGauge}m"));

                // .....
                //for (int i = 0; i < TrackToolsCreator.Previews.CachedYard.Value.SidingsLength.Length; i++)
                //{
                //    EditorGUILayout.LabelField(new GUIContent($"[{YardOptions.YardId}{YardOptions.StartTrackId + i}S]"),
                //        new GUIContent($"{TrackToolsCreator.Previews.CachedYard.Value.SidingsLength[i]}m"));
                //}
            }

            EditorGUI.indentLevel--;
        }

        private void DrawTurntableOptions()
        {
            if (!Require(TurntablePrefab, "Turntable prefab"))
            {
                return;
            }

            _turntableOptions.TurntableRadius = EditorGUILayout.FloatField(new GUIContent("Turntable radius",
                "The radius of the turntable (half the track's length)"),
                _turntableOptions.TurntableRadius);
            _turntableOptions.TurntableDepth = EditorGUILayout.FloatField(new GUIContent("Turn table depth",
                "How high up is the track compared to the bottom of the turntable."),
                _turntableOptions.TurntableDepth);
            _turntableOptions.RotationOffset = EditorGUILayout.FloatField(new GUIContent("Rotation offset",
                "Offset the turntable rotation"),
                _turntableOptions.RotationOffset);
            _turntableOptions.TracksOffset = EditorGUILayout.FloatField(new GUIContent("Track offset",
                "Offset the exit tracks"),
                _turntableOptions.TracksOffset);
            _turntableOptions.AngleBetweenExits = EditorGUILayout.FloatField(new GUIContent("Angle between exits",
                "The angle between each exit of the turntable"),
                _turntableOptions.AngleBetweenExits);
            _turntableOptions.ExitTrackCount = EditorGUILayout.IntField(new GUIContent("Exit track count",
                "Number of tracks leading away from the turntable"),
                _turntableOptions.ExitTrackCount);
            _turntableOptions.ExitTrackLength = EditorGUILayout.FloatField(new GUIContent("Exit track length",
                "Length of the straigh tracks at each exit"),
                _turntableOptions.ExitTrackLength);
        }

        private void DrawSpecialOptions()
        {
            GUI.backgroundColor *= 0.8f;
            _currentSpecial = (SpecialTrack)GUILayout.SelectionGrid((int)_currentSpecial, _specialContents, 3, EditorStyles.miniButtonMid);
            GUI.backgroundColor = Color.white;
            EditorGUILayout.Space();

            switch (_currentSpecial)
            {
                case SpecialTrack.Buffer:
                    Require(BufferPrefab, "Buffer prefab");
                    break;
                case SpecialTrack.SwitchCurve:
                    DrawSwitchCurveOptions();
                    break;
                case SpecialTrack.Connect2:
                    DrawConnect2Options();
                    break;
                case SpecialTrack.Crossover:
                    DrawCrossoverOptions();
                    break;
                case SpecialTrack.ScissorsCrossover:
                    DrawScissorsCrossoverOptions();
                    break;
                case SpecialTrack.DoubleSlip:
                    DrawDoubleSlipOptions();
                    break;
                default:
                    EditorGUILayout.HelpBox("Not implemented yet!", MessageType.Warning);
                    break;
            }
        }

        #region SPECIAL OPTIONS

        private void DrawSwitchCurveOptions()
        {
            if (!Require(LeftSwitch, "Left switch prefab") ||
                !Require(RightSwitch, "Right switch prefab"))
            {
                return;
            }

            DrawOrientationGUI("Choose which side the track diverges to");
            _connectingPoint = (SwitchPoint)EditorGUILayout.EnumPopup(new GUIContent("Connecting point",
                "Which of the 3 switch points should connect to the current track"),
                _connectingPoint);

            if (_connectingPoint == SwitchPoint.Through)
            {
                EditorGUILayout.HelpBox("The selected point has no connection with " +
                    "the track, you should select one of the others.", MessageType.Warning);
            }
        }

        private void DrawConnect2Options()
        {
            if (!Require(TrackPrefab, "Track prefab"))
            {
                return;
            }

            switch (_selectionType)
            {
                case SelectionType.Track:
                    if (_selectedTracks.Length != 2)
                    {
                        EditorGUILayout.Space();
                        EditorGUILayout.HelpBox("2 tracks should be selected!", MessageType.Error);
                        EditorGUILayout.Space();
                    }

                    EditorHelper.MultipleSelectionFoldout("Selected tracks", "Track", true, _selectedTracks, 2);
                    break;
                case SelectionType.BezierPoint:
                    if (_selectedPoints.Length != 2)
                    {
                        EditorGUILayout.Space();
                        EditorGUILayout.HelpBox("2 bezier points should be selected!", MessageType.Error);
                        EditorGUILayout.Space();
                    }

                    EditorHelper.MultipleSelectionFoldout("Selected points", "Point", true, _selectedPoints, 2);
                    break;
                default:
                    EditorGUILayout.Space();
                    EditorGUILayout.HelpBox("Select either 2 tracks or 2 points!", MessageType.Error);
                    EditorGUILayout.Space();
                    break;
            }

            _useHandle2Start = EditorGUILayout.Toggle(new GUIContent("Switch start",
                "Change the direction at the start"),
                _useHandle2Start);
            _useHandle2End = EditorGUILayout.Toggle(new GUIContent("Switch end",
                "Change the direction at the end"),
                _useHandle2End);

            _lengthMultiplier = EditorGUILayout.FloatField(
                new GUIContent("Length multiplier", "A multiplier that changes handle length, for smoother or tighter curves"),
                _lengthMultiplier);
        }

        private void DrawCrossoverOptions()
        {
            if (!Require(TrackPrefab, "Track prefab") ||
                !Require(LeftSwitch, "Left switch prefab") ||
                !Require(RightSwitch, "Right switch prefab"))
            {
                return;
            }

            DrawOrientationGUI("Which side the curve turns to");

                _trackDistance = EditorGUILayout.FloatField(new GUIContent("Track distance",
                    "The distance between parallel tracks"),
                    _trackDistance);
                _isTrailing = EditorGUILayout.Toggle(new GUIContent("Is trailing",
                    "If the crossover is trailing into this direction"),
                    _isTrailing);

                DrawSwitchDistanceGUI();
        }

        private void DrawScissorsCrossoverOptions()
        {
            if (!Require(TrackPrefab, "Track prefab") ||
                !Require(LeftSwitch, "Left switch prefab") ||
                !Require(RightSwitch, "Right switch prefab"))
            {
                return;
            }

            DrawOrientationGUI("Which side the curve turns to");

            _trackDistance = EditorGUILayout.FloatField(new GUIContent("Track distance",
                "The distance between parallel tracks"),
                _trackDistance);

            DrawSwitchDistanceGUI();

        }

        private void DrawDoubleSlipOptions()
        {
            if (!Require(TrackPrefab, "Track prefab") ||
                !Require(LeftSwitch, "Left switch prefab") ||
                !Require(RightSwitch, "Right switch prefab"))
            {
                return;
            }

            DrawOrientationGUI("Which side the curve turns to");

            float minArc = TrackToolsHelper.CalculateSwitchAngle(LeftSwitch) * Mathf.Rad2Deg * 2.0f;
            _crossAngle = EditorGUILayout.Slider(new GUIContent("Cross angle", "Angle between tracks"),
                _crossAngle, minArc + 0.1f, 90.0f);
        }

        #endregion

        #endregion

        private void DrawCreationButtons()
        {
            GUILayoutOption widthOption = GUILayout.Width(EditorGUIUtility.currentViewWidth * 0.2f);
            GUILayoutOption smallWidth = GUILayout.Width(EditorGUIUtility.currentViewWidth * 0.1f);
            // Tooltip to display, may change to explain why a button is disabled.
            string tooltip;
            bool isSwitch = CurrentTrack ? CurrentTrack.IsSwitch : false;

            EditorGUILayout.BeginVertical();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            // Back button.
            // Disable buttons in case creation isn't possible.
            GUI.enabled = IsAllowedCreation(true, out tooltip);

            if (GUILayout.Button(new GUIContent("<<<", tooltip), smallWidth))
            {
                CreateTrack(CurrentTrack.Curve[0].position, CurrentTrack.Curve[0].globalHandle2);
            }

            // Middle button.
            GUI.enabled = CurrentTrack;

            GUI.backgroundColor = EditorHelper.Cancel;
            if (GUILayout.Button(new GUIContent("Destroy", "Destroys the current track"), widthOption))
            {
                DeleteTrack();
            }

            GUI.backgroundColor = Color.white;

            // Forward button.
            GUI.enabled = IsAllowedCreation(false, out tooltip);

            if (GUILayout.Button(new GUIContent(">>>", tooltip), smallWidth))
            {
                CreateTrack(CurrentTrack.Curve.Last().position, CurrentTrack.Curve.Last().globalHandle1);
            }

            GUI.enabled = true;

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUI.backgroundColor = EditorHelper.Accept;
            if (GUILayout.Button(new GUIContent("New track", "Creates a new track"), widthOption))
            {
                CreateNewTrack();
            }

            GUI.backgroundColor = Color.white;

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void CreationFoldoutContextMenu(Rect rect)
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Reset current tab",
                "Resets the current tab's values to default"),
                false, () => ResetCreationSettings(false));
            menu.AddItem(new GUIContent("Reset all tabs",
                "Resets all tabs' values to default"),
                false, () => ResetCreationSettings(true));
            menu.DropDown(rect);
        }

        private void DrawEditingFoldout()
        {
            GUI.backgroundColor *= 1.1f;

            _showEditing = EditorGUILayout.BeginFoldoutHeaderGroup(_showEditing,
                new GUIContent("Editing", "Ways to edit tracks after they've been created"));

            GUI.backgroundColor = Color.white;

            if (_showEditing)
            {
                EditorGUILayout.Space();
                EditorGUI.indentLevel++;

                GUI.backgroundColor *= 0.8f;
                _editingMode = (EditingMode)GUILayout.SelectionGrid((int)_editingMode, _editingModeContents, 1, EditorStyles.miniButtonMid);
                GUI.backgroundColor = Color.white;
                EditorGUILayout.Space();

                switch (_editingMode)
                {
                    case EditingMode.Merge:
                        DrawTrackMerge();
                        break;
                    default:
                        EditorGUILayout.HelpBox("Coming soon!", MessageType.Info);
                        break;
                }

                EditorGUI.indentLevel--;
                EditorGUILayout.Space();

                // Only have one of the 2 open.
                _showCreation = false;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        #region EDITING OPTIONS

        private void DrawTrackMerge()
        {
            _showTracks = EditorHelper.MultipleSelectionFoldout("Selected tracks", "Track", _showTracks, _selectedTracks);

            if (_selectedTracks.Length < 2)
            {
                GUI.enabled = false;
            }

            EditorGUILayout.Space();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUI.backgroundColor = EditorHelper.Accept;

            if (GUILayout.Button("Merge", GUILayout.MaxWidth(EditorGUIUtility.labelWidth)))
            {
                if (_selectedTracks.Length > 1)
                {
                    TrackToolsEditor.MergeTracks(_selectedTracks, 0.01f, true);
                }
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUI.backgroundColor = Color.white;
            GUI.enabled = true;
        }

        #endregion

        private void DrawPrefabFoldout()
        {
            GUI.backgroundColor *= 1.1f;

            _showPrefabs = EditorGUILayout.BeginFoldoutHeaderGroup(_showPrefabs,
                new GUIContent("Prefabs", "The prefabs to be used for track creation"),
                null, PrefabFoldoutContextMenu);

            GUI.backgroundColor = Color.white;

            if (_showPrefabs)
            {
                EditorGUI.indentLevel++;

                TrackPrefab = (Track)EditorGUILayout.ObjectField(
                    new GUIContent("Track prefab"),
                    TrackPrefab, typeof(Track), true);
                BufferPrefab = (BufferStop)EditorGUILayout.ObjectField(
                    new GUIContent("Buffer prefab"),
                    BufferPrefab, typeof(BufferStop), true);
                LeftSwitch = (Switch)EditorGUILayout.ObjectField(
                    new GUIContent("Left switch prefab"),
                    LeftSwitch, typeof(Switch), true);
                RightSwitch = (Switch)EditorGUILayout.ObjectField(
                    new GUIContent("Right switch prefab"),
                    RightSwitch, typeof(Switch), true);
                TurntablePrefab = (Turntable)EditorGUILayout.ObjectField(
                    new GUIContent("Turntable prefab"),
                    TurntablePrefab, typeof(Turntable), true);

                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void PrefabFoldoutContextMenu(Rect rect)
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Get default prefabs",
                "Tries to get the default Mapify prefabs at their default location"),
                false, TryGetDefaultAssets);
            menu.AddItem(new GUIContent("Clear prefabs",
                "Sets all prefabs to null"),
                false, () => { TrackPrefab = null; BufferPrefab = null; LeftSwitch = null; RightSwitch = null; TurntablePrefab = null; });
            menu.DropDown(rect);
        }

        private void DrawSettingsFoldout()
        {
            GUI.backgroundColor *= 1.1f;

            _showSettings = EditorGUILayout.BeginFoldoutHeaderGroup(_showSettings,
                new GUIContent("Settings", "Settings to control tool looks and behaviour"),
                null, SettingsFoldoutContextMenu);

            GUI.backgroundColor = Color.white;

            if (_showSettings)
            {
                EditorGUI.indentLevel++;

                _forwardColour = EditorGUILayout.ColorField(
                    new GUIContent("Forward preview", "Colour for the forward track previews"),
                    _forwardColour);

                _backwardColour = EditorGUILayout.ColorField(
                    new GUIContent("Backward preview", "Colour for the backward track previews"),
                    _backwardColour);

                _newColour = EditorGUILayout.ColorField(
                    new GUIContent("New preview", "Colour for the new track previews"),
                    _newColour);

                _sampleCount = EditorGUILayout.IntSlider(
                    new GUIContent("Sample count", "The number of samples for track previews"),
                    _sampleCount, 2, 64);

                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void SettingsFoldoutContextMenu(Rect rect)
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Reset",
                "Resets settings to default"),
                false, ResetPreviewSettings);
            menu.DropDown(rect);
        }

        private void DrawOrientationGUI(string tooltip)
        {
            EditorGUILayout.BeginHorizontal();
            _orientation = (TrackOrientation)EditorGUILayout.EnumPopup(new GUIContent("Orientation",
                tooltip),
                _orientation);

            if (GUILayout.Button(new GUIContent("Swap orientation", "Swaps orientation between left and right"), GUILayout.MaxWidth(140)))
            {
                _orientation = FlipOrientation(_orientation);
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawSwitchDistanceGUI()
        {
            EditorGUILayout.BeginHorizontal();

            _switchDistance = EditorGUILayout.FloatField(new GUIContent("Switch distance",
                "The distance between parallel tracks"),
                _switchDistance);

            if (GUILayout.Button(new GUIContent("Straighten", "Changes the distance so the crossover track is straight"),
                GUILayout.MaxWidth(140)))
            {
                _switchDistance = TrackToolsHelper.CalculateCrossoverDistance(LeftSwitch, _trackDistance);
            }

            EditorGUILayout.EndHorizontal();
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
            switch (_currentMode)
            {
                case CreationMode.Straight:
                    SelectTrack(TrackToolsCreator.CreateStraight(TrackPrefab, _currentParent, attachPoint, handlePosition,
                        _length, _endGrade, true));
                    break;
                case CreationMode.Curve:
                    SelectTrack(TrackToolsCreator.CreateCurve(TrackPrefab, _currentParent, attachPoint, handlePosition, _orientation,
                        _radius, _arc, _maxArcPerPoint, _endGrade, true));
                    break;
                case CreationMode.Switch:
                    SelectTrack(TrackToolsCreator.CreateSwitch(LeftSwitch, RightSwitch, _currentParent, attachPoint, handlePosition,
                        _orientation, _connectingPoint, true).ThroughTrack);
                    break;
                case CreationMode.Yard:
                    SelectTrack(TrackToolsCreator.CreateYard(LeftSwitch, RightSwitch, TrackPrefab, _currentParent, attachPoint, handlePosition,
                        _orientation, _trackDistance, _yardOptions, out _, true)[1].ThroughTrack);
                    break;
                case CreationMode.Turntable:
                    SelectTrack(TrackToolsCreator.CreateTurntable(TurntablePrefab, TrackPrefab, _currentParent, attachPoint, handlePosition,
                        _turntableOptions, true, out _).Track);
                    break;
                case CreationMode.Special:
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
                case SpecialTrack.Buffer:
                    TrackToolsCreator.CreateBuffer(BufferPrefab, _currentParent, attachPoint, handlePosition, true);
                    break;
                case SpecialTrack.SwitchCurve:
                    SelectTrack(TrackToolsCreator.CreateSwitchCurve(LeftSwitch, RightSwitch, _currentParent, attachPoint, handlePosition,
                        _orientation, _connectingPoint, true));
                    break;
                case SpecialTrack.Connect2:
                    CreateConnect2();
                    break;
                case SpecialTrack.Crossover:
                    SelectTrack(TrackToolsCreator.CreateCrossover(LeftSwitch, RightSwitch, TrackPrefab, _currentParent, attachPoint, handlePosition,
                        _orientation, _trackDistance, _isTrailing, _switchDistance, true)[0].ThroughTrack);
                    break;
                case SpecialTrack.ScissorsCrossover:
                    SelectTrack(TrackToolsCreator.CreateScissorsCrossover(LeftSwitch, RightSwitch, TrackPrefab, _currentParent, attachPoint, handlePosition,
                        _orientation, _trackDistance, _switchDistance, true)[3].ThroughTrack);
                    break;
                case SpecialTrack.DoubleSlip:
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

            if (all || _currentMode == CreationMode.Straight)
            {
                _length = 100;
            }
            if (all || _currentMode == CreationMode.Curve)
            {
                _radius = 500.0f;
                _arc = 45.0f;
                _maxArcPerPoint = 22.5f;
                _changeArc = false;
            }
            if (all || _currentMode == CreationMode.Switch)
            {
                _connectingPoint = SwitchPoint.Joint;
            }
            if (all || _currentMode == CreationMode.Yard)
            {
                _yardOptions = YardOptions.DefaultOptions;
            }
            if (all || _currentMode == CreationMode.Turntable)
            {
                _turntableOptions = TurntableOptions.DefaultOptions;
            }
            if (all || _currentMode == CreationMode.Special)
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
            _forwardColour = Color.cyan;
            _backwardColour = Color.red;
            _newColour = Color.green;
            _sampleCount = 8;

            RemakeAndRepaint();
        }

        private bool IsAllowedCreation(bool isBehind, out string tooltip)
        {
            if (!CurrentTrack)
            {
                tooltip = "No selection";
                return false;
            }

            if (CheckGrade(isBehind ? CurrentTrack.GetGradeAtStart() : CurrentTrack.GetGradeAtEnd()))
            {
                tooltip = "Grade too steep for creation";
                return false;
            }

            if (CurrentTrack.IsSwitch && (_currentMode == CreationMode.Switch || _currentMode == CreationMode.Yard))
            {
                tooltip = "Cannot attach a switch to another switch directly";
                return false;
            }

            if (_currentMode == CreationMode.Special && _currentSpecial == SpecialTrack.Connect2)
            {
                tooltip = "Use the [New Track] button for this feature";
                return false;
            }

            tooltip = isBehind ? "Creates a track behind the current one" : "Creates a track in front of the current one";
            return true;
        }

        private bool CheckGrade(float grade)
        {
            switch (_currentMode)
            {
                case CreationMode.Switch:
                case CreationMode.Yard:
                case CreationMode.Turntable:
                    return Mathf.Approximately(grade, 0);
                case CreationMode.Special:
                    switch (_currentSpecial)
                    {
                        case SpecialTrack.SwitchCurve:
                        case SpecialTrack.Crossover:
                        case SpecialTrack.ScissorsCrossover:
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

            Vector3 pos;
            Vector3 forward;

            //if (_selectionType == SelectionType.BezierPoint)
            //{
            //    pos = CurrentPoint.position;
            //    forward = CurrentPoint.position - CurrentPoint.globalHandle1;
            //}
            //else
            {
                pos = _currentParent ? _currentParent.position : Vector3.zero;
                forward = _currentParent ? _currentParent.forward : Vector3.forward;
            }

            bool createFront = CurrentTrack && CheckGrade(CurrentTrack.GetGradeAtEnd());
            bool createBack = CurrentTrack && CheckGrade(CurrentTrack.GetGradeAtStart());
            bool createNew = true;

            switch (_currentMode)
            {
                case CreationMode.Straight:
                    if (createFront)
                    {
                        _forwardLines = new Vector3[][] { TrackToolsCreator.Previews.PreviewStraight(
                            CurrentTrack.Curve.Last().position,
                            CurrentTrack.Curve.Last().globalHandle1,
                            _length, _endGrade, out _forwardPoints, _sampleCount) };
                    }
                    if (createBack)
                    {
                        _backwardLines = new Vector3[][] { TrackToolsCreator.Previews.PreviewStraight(
                            CurrentTrack.Curve[0].position,
                            CurrentTrack.Curve[0].globalHandle2,
                            _length, _endGrade, out _backwardPoints, _sampleCount) };
                    }
                    if (createNew)
                    {
                        _newLines = new Vector3[][] { TrackToolsCreator.Previews.PreviewStraight(pos, pos - forward,
                            _length, _endGrade, out _newPoints, _sampleCount) };
                    }
                    break;
                case CreationMode.Curve:
                    if (createFront)
                    {
                        _forwardLines = new Vector3[][] { TrackToolsCreator.Previews.PreviewCurve(
                            CurrentTrack.Curve.Last().position, CurrentTrack.Curve.Last().globalHandle1,
                            _orientation, _radius, _arc, _maxArcPerPoint, _endGrade, out _forwardPoints, _sampleCount) };
                    }
                    if (createBack)
                    {
                        _backwardLines = new Vector3[][] { TrackToolsCreator.Previews.PreviewCurve(
                            CurrentTrack.Curve[0].position, CurrentTrack.Curve[0].globalHandle2,
                            _orientation, _radius, _arc, _maxArcPerPoint, _endGrade, out _backwardPoints, _sampleCount) };
                    }
                    if (createNew)
                    {
                        _newLines = new Vector3[][] { TrackToolsCreator.Previews.PreviewCurve(pos, pos - forward,
                            _orientation, _radius, _arc, _maxArcPerPoint, _endGrade, out _newPoints, _sampleCount) };
                    }
                    break;
                case CreationMode.Switch:
                    if (LeftSwitch && RightSwitch)
                    {
                        if (createFront)
                        {
                            _forwardPoints = new Vector3[] { CurrentTrack.Curve.Last().position };

                            _forwardLines = TrackToolsCreator.Previews.PreviewSwitch(GetCurrentSwitch(),
                                CurrentTrack.Curve.Last().position,
                                CurrentTrack.Curve.Last().globalHandle1,
                                _connectingPoint, _sampleCount);
                        }
                        if (createBack)
                        {
                            _backwardPoints = new Vector3[] { CurrentTrack.Curve[0].position };

                            _backwardLines = TrackToolsCreator.Previews.PreviewSwitch(GetCurrentSwitch(),
                                CurrentTrack.Curve[0].position,
                                CurrentTrack.Curve[0].globalHandle2,
                                _connectingPoint, _sampleCount);
                        }
                        if (createNew)
                        {
                            _newPoints = new Vector3[] { pos };

                            forward.y = 0;
                            _newLines = TrackToolsCreator.Previews.PreviewSwitch(GetCurrentSwitch(),
                                pos, pos - forward, _connectingPoint, _sampleCount);
                        }
                    }
                    break;
                case CreationMode.Yard:
                    if (LeftSwitch && RightSwitch)
                    {
                        if (createFront)
                        {
                            _forwardLines = TrackToolsCreator.Previews.PreviewYard(LeftSwitch, RightSwitch,
                                CurrentTrack.Curve.Last().position, CurrentTrack.Curve.Last().globalHandle1,
                                _orientation, _trackDistance, _yardOptions, _sampleCount);
                        }
                        if (createBack)
                        {
                            _backwardLines = TrackToolsCreator.Previews.PreviewYard(LeftSwitch, RightSwitch,
                                CurrentTrack.Curve[0].position, CurrentTrack.Curve[0].globalHandle2,
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
                case CreationMode.Turntable:
                    if (createFront)
                    {
                        _forwardLines = TrackToolsCreator.Previews.PreviewTurntable(CurrentTrack.Curve.Last().position,
                            CurrentTrack.Curve.Last().globalHandle1, _turntableOptions, _sampleCount);
                    }
                    if (createBack)
                    {
                        _backwardLines = TrackToolsCreator.Previews.PreviewTurntable(CurrentTrack.Curve[0].position,
                            CurrentTrack.Curve[0].globalHandle2, _turntableOptions, _sampleCount);
                    }
                    if (createNew)
                    {
                        forward.y = 0;
                        _newLines = TrackToolsCreator.Previews.PreviewTurntable(pos, pos - forward, _turntableOptions, _sampleCount);
                    }
                    break;
                case CreationMode.Special:
                    SpecialPreviews(createFront, createBack, createNew, pos, forward);
                    break;
                default:
                    break;
            }
        }

        private void SpecialPreviews(bool createFront, bool createBack, bool createNew, Vector3 pos, Vector3 forward)
        {
            switch (_currentSpecial)
            {
                case SpecialTrack.Buffer:
                    if (createFront)
                    {
                        _forwardPoints = new Vector3[] { CurrentTrack.Curve.Last().position };
                    }
                    if (createBack)
                    {
                        _backwardPoints = new Vector3[] { CurrentTrack.Curve[0].position };
                    }
                    if (createNew)
                    {
                        _newPoints = new Vector3[] { pos };
                    }
                    break;
                case SpecialTrack.SwitchCurve:
                    if (LeftSwitch && RightSwitch)
                    {
                        if (createFront)
                        {
                            _forwardPoints = new Vector3[] { CurrentTrack.Curve.Last().position };

                            _forwardLines = new Vector3[1][];
                            System.Array.Copy(TrackToolsCreator.Previews.PreviewSwitch(GetCurrentSwitch(),
                                CurrentTrack.Curve.Last().position,
                                CurrentTrack.Curve.Last().globalHandle1,
                                _connectingPoint, _sampleCount), 1, _forwardLines, 0, 1);
                        }
                        if (createBack)
                        {
                            _backwardPoints = new Vector3[] { CurrentTrack.Curve[0].position };

                            _backwardLines = new Vector3[1][];
                            System.Array.Copy(TrackToolsCreator.Previews.PreviewSwitch(GetCurrentSwitch(),
                                CurrentTrack.Curve[0].position,
                                CurrentTrack.Curve[0].globalHandle2,
                                _connectingPoint, _sampleCount), 1, _backwardLines, 0, 1);
                        }
                        if (createNew)
                        {
                            _newPoints = new Vector3[] { pos };

                            _newLines = new Vector3[1][];
                            forward.y = 0;
                            System.Array.Copy(TrackToolsCreator.Previews.PreviewSwitch(GetCurrentSwitch(),
                                pos, pos - forward, SwitchPoint.Joint, _sampleCount), 1, _newLines, 0, 1);
                        }
                    }
                    break;
                case SpecialTrack.Connect2:
                    if (createNew)
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
                case SpecialTrack.Crossover:
                    if (LeftSwitch && RightSwitch)
                    {
                        if (createFront)
                        {
                            _forwardPoints = new Vector3[] { CurrentTrack.Curve.Last().position };

                            _forwardLines = TrackToolsCreator.Previews.PreviewCrossover(GetCurrentSwitch(), CurrentTrack.Curve.Last().position,
                                CurrentTrack.Curve.Last().globalHandle1, _orientation, _trackDistance, _isTrailing,
                                _switchDistance, _sampleCount);
                        }
                        if (createBack)
                        {
                            _backwardPoints = new Vector3[] { CurrentTrack.Curve[0].position };

                            _backwardLines = TrackToolsCreator.Previews.PreviewCrossover(GetCurrentSwitch(), CurrentTrack.Curve[0].position,
                                CurrentTrack.Curve[0].globalHandle2, _orientation, _trackDistance, _isTrailing,
                                _switchDistance, _sampleCount);
                        }
                        if (createNew)
                        {
                            _newPoints = new Vector3[] { pos };

                            forward.y = 0;
                            _newLines = TrackToolsCreator.Previews.PreviewCrossover(GetCurrentSwitch(), pos, pos - forward,
                                _orientation, _trackDistance, _isTrailing, _switchDistance, _sampleCount);
                        }
                    }
                    break;
                case SpecialTrack.ScissorsCrossover:
                    if (LeftSwitch && RightSwitch)
                    {
                        if (createFront)
                        {
                            _forwardPoints = new Vector3[] { CurrentTrack.Curve.Last().position };

                            _forwardLines = TrackToolsCreator.Previews.PreviewScissorsCrossover(LeftSwitch, RightSwitch,
                                CurrentTrack.Curve.Last().position, CurrentTrack.Curve.Last().globalHandle1,
                                _orientation, _trackDistance, _switchDistance, _sampleCount);
                        }
                        if (createBack)
                        {
                            _backwardPoints = new Vector3[] { CurrentTrack.Curve[0].position };

                            _backwardLines = TrackToolsCreator.Previews.PreviewScissorsCrossover(LeftSwitch, RightSwitch,
                                CurrentTrack.Curve[0].position, CurrentTrack.Curve[0].globalHandle2,
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
                case SpecialTrack.DoubleSlip:
                    if (LeftSwitch && RightSwitch)
                    {
                        if (createFront)
                        {
                            _forwardPoints = new Vector3[] { CurrentTrack.Curve.Last().position };

                            _forwardLines = TrackToolsCreator.Previews.PreviewDoubleSlip(LeftSwitch, RightSwitch,
                                CurrentTrack.Curve.Last().position, CurrentTrack.Curve.Last().globalHandle1,
                                _orientation, _crossAngle, _sampleCount);
                        }
                        if (createBack)
                        {
                            _backwardPoints = new Vector3[] { CurrentTrack.Curve[0].position };

                            _backwardLines = TrackToolsCreator.Previews.PreviewDoubleSlip(LeftSwitch, RightSwitch,
                                CurrentTrack.Curve[0].position, CurrentTrack.Curve[0].globalHandle2,
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

        private Switch GetCurrentSwitch()
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
    }
}
#endif
