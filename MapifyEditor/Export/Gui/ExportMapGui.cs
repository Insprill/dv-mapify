using UnityEditor;
using UnityEngine;

namespace Mapify.Editor
{
    public class ExportMapGui : EditorWindow
    {
        private const string WINDOW_TITLE = "Export Map";

        private static ExportMapGui window;
        private static bool openFolderAfterExport;

        private bool validationRun;
        private bool validationPassed;

        [MenuItem("Mapify/Export Map")]
        public static void ShowWindow()
        {
            bool wasOpen = HasOpenInstances<ExportMapGui>();
            window = GetWindow<ExportMapGui>();
            window.Show();

            if (wasOpen) return;
            window.titleContent = new GUIContent(WINDOW_TITLE);
        }

        private void OnDestroy()
        {
            MapValidator.Cleanup();
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
            {
                GUILayout.Label(validationPassed ? "<color=green>Your map is ready to export!</color>" : "<color=maroon>Validation failed! Please fix all errors before exporting.</color>", style);
            }

            openFolderAfterExport = GUILayout.Toggle(openFolderAfterExport, "Open Folder After Export");

            GUI.enabled = validationPassed;

            if (GUILayout.Button("Export Map (Release)"))
                if (EditorUtility.DisplayDialog("Export Map",
                        "Exporting a map in release mode allows creates a smaller file size, and zips it up for distribution. " +
                        "If you want to test your map locally, export it in debug mode. " +
                        "Proceed?",
                        "Yes",
                        "No"))
                    MapExporter.OpenExportPrompt(true, openFolderAfterExport);

            if (GUILayout.Button("Export Map (Debug)"))
                if (EditorUtility.DisplayDialog("Export Map",
                        "Exporting a map in debug mode is useful when testing your map locally. " +
                        "It will create uncompressed AssetBundles that are much larger, but build & load faster. " +
                        "If you're exporting your map to distribute to others, export it in release mode. " +
                        "Proceed?",
                        "Yes",
                        "No"))
                    MapExporter.OpenExportPrompt(false, openFolderAfterExport);


            GUI.enabled = true;

            if (GUILayout.Button("Close"))
                Close();
        }
    }
}
