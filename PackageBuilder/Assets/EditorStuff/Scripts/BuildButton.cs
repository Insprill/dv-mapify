using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR

[CustomEditor(typeof(BundleBuilder))]
public class BuildButton : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var myScript = (BundleBuilder)target;
        if (GUILayout.Button("HEY! Build the bundles."))
        {
            myScript.BuildIt();
        }
    }
}

#endif

