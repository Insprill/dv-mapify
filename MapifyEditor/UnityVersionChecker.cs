#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace Mapify.Editor
{
    [InitializeOnLoad]
    public class UnityVersionChecker
    {
        private const string REQUIRED_UNITY_VERSION = "2019.4.40f1";
        private const string INSTALLATION_DOCS_URL = "https://dv-mapify.readthedocs.io/en/latest/creatingmaps/project-setup/#installing-unity";
        public static readonly string ERROR_MESSAGE =
            $"Incorrect Unity version {Application.unityVersion}! Please use {REQUIRED_UNITY_VERSION}. For more information, please check out the documentation.";

        static UnityVersionChecker()
        {
            if (IsCorrectVersion())
                return;
            if (EditorUtility.DisplayDialog("Mapify", ERROR_MESSAGE, "Open Documentation", "Ok"))
                Application.OpenURL(INSTALLATION_DOCS_URL);
        }

        public static bool IsCorrectVersion()
        {
            return Application.unityVersion == REQUIRED_UNITY_VERSION;
        }
    }
}

#endif
