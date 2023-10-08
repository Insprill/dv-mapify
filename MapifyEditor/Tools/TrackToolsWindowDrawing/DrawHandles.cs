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

            // Get some context of where we are.
            Event e = Event.current;

            // Make sure this is only done once per frame.
            // Also check for mouse up events, in order for performance mode to
            // update after something like a drag is performed.
            if (e.type == EventType.Repaint ||
                e.type == EventType.MouseUp)
            {
                // Reduce creation frequency.
                _updateCounter = (_updateCounter + 1) % _updateFrequency;

                // Force update on mouse up.
                if (e.type == EventType.MouseUp)
                {
                    _updateCounter = 0;
                }

                if (!_performanceMode || _updateCounter % _updateFrequency == 0)
                {
                    // Only check if the window is closed if the last state is open.
                    // This is here to reduce frequency of calls too.
                    if (_isOpen && !HasOpenInstances<TrackToolsWindow>())
                    {
                        _isOpen = false;
                        UnregisterEvents();
                        return;
                    }

                    if (IsInPieceMode)
                    {
                        CreatePiecePreviews();
                    }
                    else
                    {
                        ClearPreviews();
                    }
                }
            }

            // Handle stuff that needs the mouse, like the freeform tool.
            HandleMouseEvents(scene);

            // Only draw handles for track creation if the creation foldout is active.
            if (_showCreation)
            {
                DrawCreationPreviews(scene);
            }

            // Ditto, for editing.
            if (_showEditing)
            {
                DrawEditingPreviews(scene);
            }

            // Extra curve drawing.
            if (CurrentTrack)
            {
                DrawTrackExtraPreviews(scene, CurrentTrack, _zTestTrack, _sampleCount);
            }
        }

        private void DrawCreationPreviews(SceneView scene)
        {
            using (new Handles.DrawingScope(_newColour))
            {
                for (int i = 0; i < _newCache.Count; i++)
                {
                    _newCache[i].DrawLines();
                }
            }

            using (new Handles.DrawingScope(_backwardColour))
            {
                for (int i = 0; i < _backCache.Count; i++)
                {
                    _backCache[i].DrawLines();
                }
            }

            using (new Handles.DrawingScope(_forwardColour))
            {
                for (int i = 0; i < _nextCache.Count; i++)
                {
                    _nextCache[i].DrawLines();
                }
            }

            Handles.BeginGUI();

            foreach (var cache in _newCache)
            {
                using (new Handles.DrawingScope(_newColour))
                {
                    cache.DrawPointsGUI(scene);
                }

                if (!cache.DrawButton)
                {
                    continue;
                }

                GUI.enabled = cache.AllowGUI;

                if (GUI.Button(new Rect(GetButtonPositionForAttachPoint(cache.Attach), _buttonSize),
                    new GUIContent(cache.Tooltip, "New track")))
                {
                    CreateNewTrack();
                }
            }

            foreach (var cache in _backCache)
            {
                using (new Handles.DrawingScope(_backwardColour))
                {
                    cache.DrawPointsGUI(scene);
                }

                if (!cache.DrawButton)
                {
                    continue;
                }

                GUI.enabled = cache.AllowGUI;

                if (GUI.Button(new Rect(GetButtonPositionForAttachPoint(cache.Attach), _buttonSize),
                    new GUIContent(cache.Tooltip, "Previous track")))
                {
                    CreateTrack(cache.Attach);
                }
            }

            foreach (var cache in _nextCache)
            {
                using (new Handles.DrawingScope(_forwardColour))
                {
                    cache.DrawPointsGUI(scene);
                }

                if (!cache.DrawButton)
                {
                    continue;
                }

                GUI.enabled = cache.AllowGUI;

                if (GUI.Button(new Rect(GetButtonPositionForAttachPoint(cache.Attach), _buttonSize),
                    new GUIContent(cache.Tooltip, "Next track")))
                {
                    CreateTrack(cache.Attach);
                }
            }

            GUI.enabled = true;

            Handles.EndGUI();
        }

        private void DrawEditingPreviews(SceneView scene)
        {

        }

        private Vector2 GetButtonPositionForAttachPoint(AttachPoint p)
        {
            Vector2 pos = HandleUtility.WorldToGUIPoint(p.Position);
            Vector2 dir = HandleUtility.WorldToGUIPoint(p.Handle) - pos;

            // The button for each attach point is near it, moved towards its destination and upwards.
            return pos + (dir.normalized * -40.0f) - _buttonSize * 0.5f - new Vector2(0, _buttonSize.y);
        }

        private static void DrawTrackExtraPreviews(SceneView scene, Track t, bool zTest, int samples = 8)
        {
            // If there's a height change, draw the same curve but completely level.
            if (!t.Curve.IsCompletelyLevel())
            {
                float y = t.Curve[0].position.y;
                Vector3 p0, p1, p2, p3;

                using (new Handles.DrawingScope(t.Curve.drawColor.Negative()))
                {
                    p0 = t.Curve[0].position;
                    p1 = t.Curve[0].globalHandle2;

                    Handles.Label(p0 + Vector3.up * HandleUtility.GetHandleSize(p0),
                        $"{MathHelper.GetGrade(p0, p1) * 100.0f:F2}%");

                    for (int i = 1; i < t.Curve.pointCount; i++)
                    {
                        p0 = t.Curve[i - 1].position;
                        p1 = t.Curve[i - 1].globalHandle2;
                        p2 = t.Curve[i].globalHandle1;
                        p3 = t.Curve[i].position;

                        Handles.Label(p3 + Vector3.up * HandleUtility.GetHandleSize(p3),
                            $"{MathHelper.GetGrade(p2, p3) * 100.0f:F2}%");

                        p0.y = y;
                        p1.y = y;
                        p2.y = y;
                        p3.y = y;

                        EditorHelper.DrawBezier(p0, p1, p2, p3, samples);
                        Handles.DrawLine(p3, t.Curve[i].position);
                    }
                }
            }

            if (zTest)
            {
                using (new Handles.DrawingScope(t.Curve.drawColor.Negative()))
                {
                    var ztest = Handles.zTest;
                    Handles.zTest = UnityEngine.Rendering.CompareFunction.Greater;

                    for (int i = 1; i < t.Curve.pointCount; i++)
                    {
                        EditorHelper.DrawBezier(
                            t.Curve[i - 1].position,
                            t.Curve[i - 1].globalHandle2,
                            t.Curve[i].globalHandle1,
                            t.Curve[i].position,
                            samples);
                    }

                    Handles.zTest = ztest;
                }
            }
        }

        public static void DrawCurveGrades(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, bool includeFirst, int samples)
        {
            Vector3[] results = MathHelper.SampleBezier(p0, p1, p2, p3, samples);
            Vector3 tan;
            Vector3 flatSample;
            GUIContent label;
            Rect rect;

            // Draw vertical lines.
            Handles.color = Color.black;
            for (int i =  1; i < results.Length; i++)
            {
                flatSample = results[i];
                flatSample.y = results[0].y;
                Handles.DrawLine(results[i], flatSample);
            }
            Handles.color = Color.white;

            Handles.BeginGUI();

            for (int i = includeFirst ? 0 : 1; i < results.Length; i++)
            {
                tan = BezierCurve.Tangent(p0, p1, p2, p3, i / (float)samples);
                //Handles.Label(results[i] + Vector3.up * HandleUtility.GetHandleSize(results[i]), new GUIContent(
                //    $"{MathHelper.GetGrade(results[i], results[i] + tan) * 100.0f:F2}%"));

                label = new GUIContent(new GUIContent($"{MathHelper.GetGrade(Vector3.zero, tan) * 100.0f:F2}%"));
                rect = HandleUtility.WorldPointToSizedRect(results[i], label, GUI.skin.label);
                rect.position += new Vector2(-20, -30);
                GUI.Label(rect, label);
            }

            Handles.EndGUI();
        }

        public static void DrawCurveGrades(Vector3[] curve, bool includeFirst, int samples)
        {
            DrawCurveGrades(curve[0], curve[1], curve[2], curve[3], includeFirst, samples);
        }
    }
}
#endif
