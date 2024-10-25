#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Mapify.Editor
{
    public class ExportMapGui : EditorWindow
    {
        private const string WINDOW_TITLE = "Export Map";

        private static ExportMapGui window;

        private bool validationRun;
        private bool validationPassed;

        [MenuItem("Mapify/Export Map", priority = 1)]
        public static void ShowWindow()
        {
            bool wasOpen = HasOpenInstances<ExportMapGui>();
            window = GetWindow<ExportMapGui>();
            window.Show();

            if (wasOpen) return;
            window.titleContent = new GUIContent(WINDOW_TITLE);
        }

        private void OnGUI()
        {
            GUIStyle style = new GUIStyle(GUI.skin.label) {
                richText = true
            };

            if (!validationRun)
                GUILayout.Label("Please validate your map before exporting");

            if (GUILayout.Button("Validate"))
            {
                validationRun = true;
                validationPassed = MapValidationGui.OpenAndValidate();
            }

            if (validationRun)
                GUILayout.Label(
                    validationPassed
                        ? $"<color={MapValidationGui.SUCCESS_COLOR}>Your map is ready to export!</color>"
                        : $"<color={MapValidationGui.ERROR_COLOR}>Validation failed! Please fix all errors before exporting.</color>", style
                );

            GUI.enabled = validationPassed;
            EditorStyles.label.wordWrap = true;

            GUILayout.Space(20);
            EditorGUILayout.LabelField(
                "Exporting a map in release mode creates a smaller file size, and zips it up for distribution. " +
                "If you want to test your map locally, export it in debug mode. "
            );
            if (GUILayout.Button("Export Map (Release)"))
                MapExporter.OpenExportPrompt(true);

            GUILayout.Space(10);
            EditorGUILayout.LabelField(
                "Exporting a map in debug mode is useful when testing your map locally. " +
                "It will create uncompressed AssetBundles that are much larger, but build & load faster. " +
                "If you're exporting your map to distribute to others, export it in release mode. "
            );
            if (GUILayout.Button("Export Map (Debug)"))
                MapExporter.OpenExportPrompt(false);

            GUI.enabled = true;


            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Close"))
                Close();
        }
    }
}
#endif
