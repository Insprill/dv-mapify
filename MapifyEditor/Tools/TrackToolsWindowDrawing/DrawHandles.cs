using Mapify.Editor.Utils;
using UnityEditor;
using UnityEngine;
using static Mapify.Editor.Tools.ToolEnums;

#if UNITY_EDITOR
namespace Mapify.Editor.Tools
{
    public partial class TrackToolsWindow
    {
        private Vector2 _buttonSize = new Vector2(30, 30);

        private void DrawHandles(SceneView scene)
        {
            // Don't draw if the window is closed.
            if (!_isOpen)
            {
                return;
            }

            // Reduce creation frequency.
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

                CreatePreviews();
            }

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
                DrawTrackExtraPreviews();
            }
        }

        private void DrawCreationPreviews()
        {
            using (new Handles.DrawingScope(_newColour))
            {
                for (int i = 0; i < _newCache.Count; i++)
                {
                    _newCache[i].Draw();
                }
            }

            using (new Handles.DrawingScope(_backwardColour))
            {
                for (int i = 0; i < _backCache.Count; i++)
                {
                    _backCache[i].Draw();
                }
            }

            using (new Handles.DrawingScope(_forwardColour))
            {
                for (int i = 0; i < _nextCache.Count; i++)
                {
                    _nextCache[i].Draw();
                }
            }

            Handles.BeginGUI();

            for (int i = 0; i < _newCache.Count; i++)
            {
                if (!_newCache[i].DrawButton)
                {
                    continue;
                }

                if (GUI.Button(new Rect(GetButtonPositionForAttachPoint(_newCache[i].Attach), _buttonSize),
                    new GUIContent(_newCache[i].Tooltip, "New track")))
                {
                    CreateNewTrack();
                }
            }

            for (int i = 0; i < _backCache.Count; i++)
            {
                if (!_backCache[i].DrawButton)
                {
                    continue;
                }

                if (GUI.Button(new Rect(GetButtonPositionForAttachPoint(_backCache[i].Attach), _buttonSize),
                    new GUIContent(_backCache[i].Tooltip, "Previous track")))
                {
                    CreateTrack(_backCache[i].Attach);
                }
            }

            for (int i = 0; i < _nextCache.Count; i++)
            {
                if (!_nextCache[i].DrawButton)
                {
                    continue;
                }

                if (GUI.Button(new Rect(GetButtonPositionForAttachPoint(_nextCache[i].Attach), _buttonSize),
                    new GUIContent(_nextCache[i].Tooltip, "Next track")))
                {
                    CreateTrack(_nextCache[i].Attach);
                }
            }

            Handles.EndGUI();
        }

        private void DrawEditingPreviews()
        {

        }

        private void DrawTrackExtraPreviews()
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

        private Vector2 GetButtonPositionForAttachPoint(AttachPoint p)
        {
            Vector2 pos = HandleUtility.WorldToGUIPoint(p.Position);
            Vector2 dir = HandleUtility.WorldToGUIPoint(p.Handle) - pos;

            // The button for each attach point is near it, moved towards its destination and upwards.
            return pos + (dir.normalized * -40.0f) - _buttonSize * 0.5f - new Vector2(0, _buttonSize.y);
        }
    }
}
#endif
