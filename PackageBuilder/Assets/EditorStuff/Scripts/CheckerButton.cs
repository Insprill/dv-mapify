using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR

[CustomEditor(typeof(BundleChecker))]
public class CheckerButton : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var myScript = (BundleChecker)target;
        if (GUILayout.Button("Check em"))
        {
            myScript.CheckEm();
        }
    }
}

#endif