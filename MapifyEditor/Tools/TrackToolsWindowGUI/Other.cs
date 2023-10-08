using Mapify.Editor.Utils;
using UnityEditor;
using UnityEngine;
using static Mapify.Editor.Tools.ToolEnums;

#if UNITY_EDITOR
namespace Mapify.Editor.Tools
{
    public partial class TrackToolsWindow
    {
        // Track prefabs.
        private bool _showPrefabs = false;
        public BufferStop BufferPrefab;
        public Switch LeftSwitch;
        public Switch RightSwitch;
        public Turntable TurntablePrefab;

        // Settings.
        private bool _showSettings = false;
        private bool _drawNewPreview = true;
        private bool _performanceMode = false;
        private bool _zTestTrack = true;
        private Color _forwardColour = Color.cyan;
        private Color _backwardColour = Color.red;
        private Color _newColour = Color.green;
        private int _sampleCount = 8;

        // Foldout with the 5 prefabs used for track creation.
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

                BufferPrefab = EditorHelper.ObjectField(
                    new GUIContent("Buffer prefab"),
                    BufferPrefab, true);
                LeftSwitch = EditorHelper.ObjectField(
                    new GUIContent("Left switch prefab"),
                    LeftSwitch, true);
                RightSwitch = EditorHelper.ObjectField(
                    new GUIContent("Right switch prefab"),
                    RightSwitch, true);
                TurntablePrefab = EditorHelper.ObjectField(
                    new GUIContent("Turntable prefab"),
                    TurntablePrefab, true);

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
                false, () => { BufferPrefab = null; LeftSwitch = null; RightSwitch = null; TurntablePrefab = null; });
            menu.DropDown(rect);
        }

        // Foldout with the tool settings.
        private void DrawSettingsFoldout()
        {
            GUI.backgroundColor *= 1.1f;

            _showSettings = EditorGUILayout.BeginFoldoutHeaderGroup(_showSettings,
                new GUIContent("Tool settings", "Settings to control tool looks and behaviour"),
                null, SettingsFoldoutContextMenu);

            GUI.backgroundColor = Color.white;

            if (_showSettings)
            {
                EditorGUI.indentLevel++;

                _drawNewPreview = EditorGUILayout.Toggle(
                    new GUIContent("New track preview", "Show or hide the new track preview"),
                    _drawNewPreview);

                _performanceMode = EditorGUILayout.Toggle(
                    new GUIContent("Performance mode", "Reduces redraw frequency"),
                    _performanceMode);

                _zTestTrack = EditorGUILayout.Toggle(
                    new GUIContent("Perform Z test", "Draw parts of the selected track that are behind objects as a different colour"),
                    _zTestTrack);

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

        // Orientation enum dropdown with a button to swap options easily.
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

        // SwitchPoint enum dropdown with a button to swap options easily.
        private void DrawSwitchPointGUI()
        {
            EditorGUILayout.BeginHorizontal();
            _connectingPoint = (SwitchPoint)EditorGUILayout.EnumPopup(new GUIContent("Connecting point",
                "Which of the 3 switch points should connect to the current track"),
                _connectingPoint);

            if (GUILayout.Button(new GUIContent("Next point", "Swaps between the 3 switch points."), GUILayout.MaxWidth(140)))
            {
                _connectingPoint = NextPoint(_connectingPoint);
            }

            EditorGUILayout.EndHorizontal();
        }

        // Orientation enum dropdown with a button to swap options easily.
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
    }
}
#endif
