using Mapify.Editor.Utils;
using UnityEditor;
using UnityEngine;
using static Mapify.Editor.Tools.ToolEnums;

#if UNITY_EDITOR
namespace Mapify.Editor.Tools
{
    public partial class TrackToolsWindow
    {
        private bool _showEditing = false;
        private EditingMode _editingMode = EditingMode.Merge;
        private int _editStartIndex = 0;
        private int _editEndIndex = 1;
        private float _editPercent = 0.5f;

        // Terrain match.
        private float _maxDistance = 500.0f;
        private float _reverseOffset = 5.0f;

        // InsertPoint

        // Editing mode.
        private readonly GUIContent[] _editingModeContents = {
            new GUIContent("Merge", "Merges multiple tracks into a one"),
            new GUIContent("Terrain match", "Matches tracks to terrain and other objects"),
            new GUIContent("Insert point", "Inserts a point between 2 others")
        };

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
                _editingMode = (EditingMode)GUILayout.SelectionGrid((int)_editingMode, _editingModeContents, 3, EditorStyles.miniButtonMid);
                GUI.backgroundColor = Color.white;
                EditorGUILayout.Space();

                switch (_editingMode)
                {
                    case EditingMode.Merge:
                        DrawTrackMerge();
                        break;
                    case EditingMode.MatchTerrain:
                        DrawMatchTerrain();
                        break;
                    case EditingMode.InsertPoint:
                        DrawInsertPoint();
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

        private void DrawMatchTerrain()
        {
            _heightOffset = EditorGUILayout.FloatField(
                new GUIContent("Height offset", "The height at which the track is placed, above the terrain"),
                _heightOffset);
            _maxDistance = EditorGUILayout.FloatField(
                new GUIContent("Max distance", "How far each point can be moved downwards"),
                _maxDistance);
            _reverseOffset = EditorGUILayout.FloatField(
                new GUIContent("Reverse offset", "How far above each point terrain will be considered"),
                _reverseOffset);
            _fixToNormal = EditorGUILayout.Slider(
                new GUIContent("Use collision normals", "How the grade should match the terrain, 0 is keeping the original handles " +
                "and 1 being perfectly matching the terrain position"),
                _fixToNormal, 0.0f, 1.0f);

            _showTracks = EditorHelper.MultipleSelectionFoldout("Selected tracks", "Track", _showTracks, _selectedTracks);

            EditorGUILayout.Space();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUI.backgroundColor = EditorHelper.Accept;
            GUI.enabled = _selectedTracks.Length > 1;

            if (GUILayout.Button("Match", GUILayout.MaxWidth(EditorGUIUtility.labelWidth)))
            {
                for (int i = 0; i < _selectedTracks.Length; i++)
                {
                    TrackToolsEditor.MatchTrackToTerrain(_selectedTracks[i],
                        _heightOffset,
                        _maxDistance,
                        _reverseOffset,
                        _fixToNormal);
                }
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUI.backgroundColor = Color.white;
            GUI.enabled = true;
        }

        private void DrawInsertPoint()
        {
            if (!Require(CurrentTrack, "Selected track"))
            {
                return;
            }

            (_editStartIndex, _editEndIndex) = EditorHelper.MinMaxSliderInt(
                new GUIContent("Split points", "The 2 points between which the insertion will happen"),
                _editStartIndex, _editEndIndex, 0, CurrentTrack.Curve.pointCount - 1);

            if (_editStartIndex >= _editEndIndex)
            {
                EditorGUILayout.HelpBox("The 2 points cannot be the same!", MessageType.Error);
            }

            _editPercent = EditorGUILayout.Slider(
                new GUIContent("Point in curve"),
                _editPercent, 0.0f, 1.0f);

            _editStartIndex = Mathf.Min(_editStartIndex, CurrentTrack.Curve.pointCount - 2);

            EditorGUILayout.Space();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUI.backgroundColor = EditorHelper.Accept;

            if (GUILayout.Button("Insert", GUILayout.MaxWidth(EditorGUIUtility.labelWidth)))
            {
                for (int i = _editEndIndex - 1; i >= _editStartIndex; i--)
                {
                    TrackToolsEditor.CreatePointBetween2(CurrentTrack, i, _editPercent);
                }
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUI.backgroundColor = Color.white;
        }

        #endregion
    }
}
#endif
