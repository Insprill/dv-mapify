using System;
using Mapify.Editor.Tools.OptionData;
using Mapify.Editor.Utils;
using UnityEditor;
using UnityEngine;
using static Mapify.Editor.Tools.ToolEnums;

#if UNITY_EDITOR
namespace Mapify.Editor.Tools
{
    public partial class TrackToolsWindow
    {
        private bool _showCreation = true;
        private CreationMode _creationMode = CreationMode.Piece;
        private TrackPiece _currentPiece = TrackPiece.Straight;

        // Global options.
        private Transform _currentParent;
        private TrackAge _trackAge = TrackAge.New;
        private bool _generateSigns = true;
        private bool _generateBallast = true;
        private bool _generateSleepers = true;

        // Options used in multiple pieces.
        private TrackOrientation _orientation;
        private float _endGrade = 0.0f;
        private float _trackDistance = 4.5f;

        // Freeform.
        private float _heightOffset = 0.5f;
        private float _smoothMix = 1.0f;
        private float _fixToNormal = 1.0f;
        private bool _showSnapPoint = true;
        private bool _quickBuild = true;
        private bool _creating = false;

        // Straight.
        private float _length = 100.0f;

        // Curves.
        private float _radius = 500.0f;
        private float _arc = 45.0f;
        private float _maxArcPerPoint = 22.5f;
        private bool _changeArc = false;

        // Switches.
        private SwitchType _switchType = SwitchType.Vanilla;
        private int _switchBranchesCount = 2;
        private SwitchPoint _connectingPointVanilla = SwitchPoint.Joint;
        // 0 -> joint point, 1 - ∞ -> branch point
        private int _connectingPointCustom = 0;

        // Yards.
        private YardOptions _yardOptions = YardOptions.DefaultOptions;
        private bool _showYardCache = false;

        // Turntable.
        private TurntableOptions _turntableOptions = TurntableOptions.DefaultOptions;

        // Special.
        private SpecialTrackPiece _currentSpecial = SpecialTrackPiece.Buffer;
        private bool _useHandle2Start = false;
        private bool _useHandle2End = false;
        private float _lengthMultiplier = 1.0f;
        private bool _isTrailing = false;
        private float _switchDistance = 6.0f;
        private float _crossAngle = 20.0f;

        // Contents for the tool selection.
        private readonly GUIContent[] _creationModeContents =
        {
            new GUIContent("Freeform", "Freeform track creation, useful for normal track laying"),
            new GUIContent("Pieces", "Procedurally generated pieces, useful for more perfect operations")
        };

        // Contents for the piece selection.
        private readonly GUIContent[] _pieceContents = {
            new GUIContent("Straight", "Straight tracks with a custom length"),
            new GUIContent("Curve", "Curves that approximate a circular arc"),
            new GUIContent("Switch", "Track switches"),
            new GUIContent("Yard", "Yards and sidings"),
            new GUIContent("Turntable", "Turntables"),
            new GUIContent("Special", "Includes multiple track pieces including:\n- Buffers\n- Intersections/Crossovers\nAnd more!")
        };

        // Contents for special track selection.
        private readonly GUIContent[] _specialContents = {
            new GUIContent("Buffer", "A buffer stop at the end of a track"),
            new GUIContent("Switch curve", "The curve used by switches"),
            new GUIContent("Connect 2", "Connect 2 bezier points smoothly"),
            new GUIContent("Crossover", "A pair of switches that allows changing between 2 parallel tracks"),
            new GUIContent("Scissors crossover", "2 crossovers at the same time"),
            new GUIContent("Double slip", "A switch arrangement often found near stations")
        };

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

                DrawGlobalOptions();

                GUI.backgroundColor *= 0.8f;
                _creationMode = (CreationMode)GUILayout.SelectionGrid((int)_creationMode, _creationModeContents, 2, GUI.skin.button);
                GUI.backgroundColor = Color.white;
                EditorHelper.Separator();
                EditorGUILayout.Space();

                switch (_creationMode)
                {
                    case CreationMode.Freeform:
                        DrawCreationFreeformOptions();
                        break;
                    case CreationMode.Piece:
                        DrawCreationPieceOptions();
                        break;
                    default:
                        NotImplementedGUI();
                        break;
                }

                EditorGUI.indentLevel--;
                EditorGUILayout.Space();

                // Only have one of the 2 open.
                _showEditing = false;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawGlobalOptions()
        {
            _currentParent = EditorHelper.ObjectField(
                new GUIContent("Track parent", "The parent transform for new tracks"),
                _currentParent, true);
            _trackAge = (TrackAge)EditorGUILayout.EnumPopup(
                new GUIContent("Track age"),
                _trackAge);
            _generateSigns = EditorGUILayout.Toggle(
                new GUIContent("Generate signs"),
                _generateSigns);
            _generateBallast = EditorGUILayout.Toggle(
                new GUIContent("Generate ballast"),
                _generateBallast);
            _generateSleepers = EditorGUILayout.Toggle(
                new GUIContent("Generate sleepers"),
                _generateSleepers);

            EditorGUILayout.Space();
        }

        private void DrawCreationFreeformOptions()
        {
            GUILayoutOption widthOption = GUILayout.Width(EditorGUIUtility.currentViewWidth * 0.2f);

            _heightOffset = EditorGUILayout.FloatField(
                new GUIContent("Height offset", "The height at which the track is placed, above the clicked point"),
                _heightOffset);
            _smoothMix = EditorGUILayout.Slider(
                new GUIContent("Smooth mode", "How the smoothing between points should be calculated"),
                _smoothMix, 0.0f, 1.0f);
            _fixToNormal = EditorGUILayout.Slider(
                new GUIContent("Use collision normals", "How the grade should match the clicked position, 0 being perfectly smooth to the curve " +
                "and 1 being perfectly matching the clicked position"),
                _fixToNormal, 0.0f, 1.0f);
            _lengthMultiplier = EditorGUILayout.FloatField(
                new GUIContent("Length multiplier", "A multiplier that changes handle length, for smoother or tighter curves"),
                _lengthMultiplier);
            _showSnapPoint = EditorGUILayout.Toggle(
                new GUIContent("Show snap point", "Show where the click will snap to."),
                _showSnapPoint);
            _quickBuild = EditorGUILayout.Toggle(
                new GUIContent("Quick build", "Instantly lay new track on click."),
                _quickBuild);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (_creating)
            {
                GUI.backgroundColor *= EditorHelper.Cancel;

                if (GUILayout.Button(new GUIContent("Stop", "Concludes the track creation"), widthOption))
                {
                    StopFreeform();
                }
            }
            else
            {
                GUI.backgroundColor *= EditorHelper.Accept;

                if (GUILayout.Button(new GUIContent("Start", "Begins the track creation"), widthOption))
                {
                    StartFreeform();
                }
            }

            GUI.backgroundColor = Color.white;

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private void StartFreeform()
        {
            TrackToolsHelper.CreateCache();
            _creating = true;
        }

        private void StopFreeform()
        {
            if (_freeformTrackHelper != null && _freeformTrackHelper.UndoIndex.HasValue)
            {
                Undo.CollapseUndoOperations(_freeformTrackHelper.UndoIndex.Value);
            }

            if (_freeformTrackHelper != null && _freeformTrackHelper.WorkingTrack)
            {
                Selection.activeGameObject = _freeformTrackHelper.WorkingTrack.gameObject;
            }

            _freeformTrackHelper = null;
            _creating = false;
        }

        private void DrawCreationPieceOptions()
        {
            // Select the current editing mode.
            GUI.backgroundColor *= 0.8f;
            _currentPiece = (TrackPiece)GUILayout.SelectionGrid((int)_currentPiece, _pieceContents, 6, EditorStyles.miniButtonMid);
            GUI.backgroundColor = Color.white;
            EditorGUILayout.Space();

            switch (_currentPiece)
            {
                case TrackPiece.Straight:
                    DrawStraightOptions();
                    break;
                case TrackPiece.Curve:
                    DrawCurveOptions();
                    break;
                case TrackPiece.Switch:
                    DrawSwitchOptions();
                    break;
                case TrackPiece.Yard:
                    DrawYardOptions();
                    break;
                case TrackPiece.Turntable:
                    DrawTurntableOptions();
                    break;
                case TrackPiece.Special:
                    DrawSpecialOptions();
                    break;
                default:
                    NotImplementedGUI();
                    break;
            }

            EditorGUILayout.Space();
            DoNullCheck();
            DrawCreationButtons();
        }

        #region PIECE CREATION OPTIONS

        private void DrawStraightOptions()
        {
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

        private void DrawCurveOptions(bool CustomSwitchBranch = false)
        {
            if (!CustomSwitchBranch)
            {
                DrawOrientationGUI("Which side the curve turns to");
            }

            EditorGUILayout.BeginHorizontal();

            _radius = EditorGUILayout.FloatField(new GUIContent("Radius", "Radius of the curve"),
                _radius);

            if (GUILayout.Button(new GUIContent("Use switch radius", "Sets the radius to the one of switch curves"),
                GUILayout.MaxWidth(140)))
            {
                if (CustomSwitchBranch)
                {
                    _radius = TrackToolsHelper.DefaultSwitchRadius;
                }
                else
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
            }

            EditorGUILayout.EndHorizontal();

            _arc = EditorGUILayout.Slider(new GUIContent("Arc", "Angle of the curve"),
                _arc, 0.1f, 180.0f);
            if (!CustomSwitchBranch)
            {
                _maxArcPerPoint = EditorGUILayout.Slider(new GUIContent("Max arc per point",
                        "How big an arc can be before the curve is split."),
                    _maxArcPerPoint, 0.0f, 90.0f);
            }

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
            //length of zero makes no sense and will cause exceptions
            if(length < 0.1f) length = 0.1f;

            bool changed = EditorGUI.EndChangeCheck();

            _changeArc = EditorGUILayout.ToggleLeft(new GUIContent("Change arc",
                "Change the arc of the curve instead of the radius to match the length"),
                _changeArc, GUILayout.MaxWidth(140));

            if (changed)
            {
                if (_changeArc)
                {
                    _arc = length / _radius * Mathf.Rad2Deg;
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
            _switchType = (SwitchType)EditorGUILayout.EnumPopup(new GUIContent("Switch type",
                    "Vanilla switch (like in the base game) or a custom switch (create your own shape)"),
                _switchType);

            switch (_switchType)
            {
                case SwitchType.Vanilla:
                    DrawVanillaSwitchOptions();
                    break;
                case SwitchType.Custom:
                    DrawCustomSwitchOptions();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void DrawYardOptions()
        {
            if (!Require(LeftSwitch, "Left switch prefab") ||
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
            _yardOptions.Half = EditorGUILayout.Toggle(new GUIContent("Half yard",
                "If true, the yard will only have 1 exit"),
                _yardOptions.Half);
            GUI.enabled = !_yardOptions.Half;
            _yardOptions.AlternateSides = EditorGUILayout.Toggle(new GUIContent("Alternate sides",
                "If true, the switches at either end will turn to different sides of the yard, if false they will " +
                "instead face the same side"),
                _yardOptions.AlternateSides);
            GUI.enabled = true;
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
            _yardOptions.ReverseNumbers = EditorGUILayout.Toggle(new GUIContent("Reverse track numbers",
                "If true, the track numbers will instead decrease"),
                _yardOptions.ReverseNumbers);

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
            _currentSpecial = (SpecialTrackPiece)GUILayout.SelectionGrid((int)_currentSpecial, _specialContents, 3, EditorStyles.miniButtonMid);
            GUI.backgroundColor = Color.white;
            EditorGUILayout.Space();

            switch (_currentSpecial)
            {
                case SpecialTrackPiece.Buffer:
                    Require(BufferPrefab, "Buffer prefab");
                    break;
                case SpecialTrackPiece.SwitchCurve:
                    DrawSwitchCurveOptions();
                    break;
                case SpecialTrackPiece.Connect2:
                    DrawConnect2Options();
                    break;
                case SpecialTrackPiece.Crossover:
                    DrawCrossoverOptions();
                    break;
                case SpecialTrackPiece.ScissorsCrossover:
                    DrawScissorsCrossoverOptions();
                    break;
                case SpecialTrackPiece.DoubleSlip:
                    DrawDoubleSlipOptions();
                    break;
                default:
                    NotImplementedGUI();
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
            DrawVanillaSwitchPointGUI();

            if (_connectingPointVanilla == SwitchPoint.Through)
            {
                EditorGUILayout.HelpBox("The selected point has no connection with " +
                    "the track, you should select one of the others.", MessageType.Warning);
            }
        }

        private void DrawConnect2Options()
        {
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
            if (!Require(LeftSwitch, "Left switch prefab") ||
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
            if (!Require(LeftSwitch, "Left switch prefab") ||
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
            if (!Require(LeftSwitch, "Left switch prefab") ||
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
                switch (_selectionType)
                {
                    case SelectionType.Track:
                        CreateTrack(CurrentTrack.Curve[0].position, CurrentTrack.Curve[0].globalHandle2);
                        break;
                    case SelectionType.BezierPoint:
                        CreateTrack(CurrentPoint.position, CurrentPoint.globalHandle2);
                        break;
                    default:
                        break;
                }
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
                switch (_selectionType)
                {
                    case SelectionType.Track:
                        CreateTrack(CurrentTrack.Curve.Last().position, CurrentTrack.Curve.Last().globalHandle1);
                        break;
                    case SelectionType.BezierPoint:
                        CreateTrack(CurrentPoint.position, CurrentPoint.globalHandle1);
                        break;
                    default:
                        break;
                }
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
    }
}
#endif
