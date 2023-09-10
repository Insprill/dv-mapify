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

        // Editing mode.
        private readonly GUIContent[] _editingModeContents = {
            new GUIContent("Merge", "Merges multiple tracks into a one"),
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
    }
}
#endif
