#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Mapify.Editor
{
    [CustomEditor(typeof(SplitVisualizer))]
    public class SplitVisualizerEditor : UnityEditor.Editor
    {
        private SplitVisualizer visualizer;

        private void OnEnable()
        {
            visualizer = (SplitVisualizer)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Update Renderers"))
                visualizer.renderers = FindObjectsOfType<Renderer>();
        }
    }
}
#endif
