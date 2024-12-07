/*

    This script was adapted from the free Unity Asset Store asset "Bezier Curve Editor".
    The original asset can be found at https://assetstore.unity.com/packages/tools/11278
    © 2015 Arkham Interactive

*/

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Mapify.Editor.BezierCurves
{
    [CustomEditor(typeof(BezierCurve))]
    public class BezierCurveEditor : UnityEditor.Editor
    {
        private BezierCurve curve;
        private SerializedProperty resolutionProp;
        private SerializedProperty closeProp;
        private SerializedProperty pointsProp;
        private SerializedProperty colorProp;

        private static bool showPoints = true;

        private void OnEnable()
        {
            curve = (BezierCurve)target;

            resolutionProp = serializedObject.FindProperty("resolution");
            closeProp = serializedObject.FindProperty("_close");
            pointsProp = serializedObject.FindProperty("points");
            colorProp = serializedObject.FindProperty("drawColor");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // These shouldn't be changed by the user, so don't even draw them
            resolutionProp.floatValue = 0.5f;
            closeProp.boolValue = false;

            EditorGUILayout.PropertyField(colorProp);

            showPoints = EditorGUILayout.Foldout(showPoints, "Points");

            if (showPoints)
            {
                int pointCount = pointsProp.arraySize;

                for (int i = 0; i < pointCount; i++)
                {
                    if (curve[i] == null) curve.RemovePoint(i);
                    DrawPointInspector(curve[i], i);
                }

                if (GUILayout.Button("Add Point"))
                {
                    GameObject pointObject = new GameObject($"Point {pointsProp.arraySize}") {
                        transform = {
                            parent = curve.transform,
                            localPosition = Vector3.zero
                        }
                    };

                    BezierPoint newPoint = pointObject.AddComponent<BezierPoint>();
                    newPoint.curve = curve;
                    newPoint.handle1 = Vector3.right * 0.1f;
                    newPoint.handle2 = -Vector3.right * 0.1f;

                    Undo.RegisterCreatedObjectUndo(pointObject, "Add Point");

                    pointsProp.InsertArrayElementAtIndex(pointsProp.arraySize);
                    pointsProp.GetArrayElementAtIndex(pointsProp.arraySize - 1).objectReferenceValue = newPoint;
                }
            }

            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
            }
        }

        private void OnSceneGUI()
        {
            for (int i = 0; i < curve.pointCount; i++) DrawPointSceneGUI(curve[i]);
        }

        private void DrawPointInspector(BezierPoint point, int index)
        {
            SerializedObject serObj = new SerializedObject(point);

            SerializedProperty handleStyleProp = serObj.FindProperty("handleStyle");
            SerializedProperty handle1Prop = serObj.FindProperty("_handle1");
            SerializedProperty handle2Prop = serObj.FindProperty("_handle2");

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                if (PrefabUtility.IsPartOfPrefabInstance(point))
                {
                    EditorUtility.DisplayDialog("Mapify", "You cannot remove a Bezier Point that's part of a prefab! Please unpack it completely before trying again.", "Ok");
                    return;
                }

                pointsProp.MoveArrayElement(curve.GetPointIndex(point), curve.pointCount - 1);
                pointsProp.arraySize--;
                Undo.DestroyObjectImmediate(point.gameObject);
                return;
            }

            EditorGUILayout.ObjectField(point.gameObject, typeof(GameObject), true);

            if (index != 0 && GUILayout.Button("↑", GUILayout.Width(25)))
            {
                Object other = pointsProp.GetArrayElementAtIndex(index - 1).objectReferenceValue;
                pointsProp.GetArrayElementAtIndex(index - 1).objectReferenceValue = point;
                pointsProp.GetArrayElementAtIndex(index).objectReferenceValue = other;
            }

            if (index != pointsProp.arraySize - 1 && GUILayout.Button("↓", GUILayout.Width(25)))
            {
                Object other = pointsProp.GetArrayElementAtIndex(index + 1).objectReferenceValue;
                pointsProp.GetArrayElementAtIndex(index + 1).objectReferenceValue = point;
                pointsProp.GetArrayElementAtIndex(index).objectReferenceValue = other;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel++;
            EditorGUI.indentLevel++;

            int newType = (int)(object)EditorGUILayout.EnumPopup("Handle Type", (BezierPoint.HandleStyle)handleStyleProp.enumValueIndex);

            if (newType != handleStyleProp.enumValueIndex)
            {
                handleStyleProp.enumValueIndex = newType;
                switch (newType)
                {
                    case 0 when handle1Prop.vector3Value != Vector3.zero:
                        handle2Prop.vector3Value = -handle1Prop.vector3Value;
                        break;
                    case 0 when handle2Prop.vector3Value != Vector3.zero:
                        handle1Prop.vector3Value = -handle2Prop.vector3Value;
                        break;
                    case 0:
                        handle1Prop.vector3Value = new Vector3(0.1f, 0, 0);
                        handle2Prop.vector3Value = new Vector3(-0.1f, 0, 0);
                        break;
                    case 1: {
                        if (handle1Prop.vector3Value == Vector3.zero && handle2Prop.vector3Value == Vector3.zero)
                        {
                            handle1Prop.vector3Value = new Vector3(0.1f, 0, 0);
                            handle2Prop.vector3Value = new Vector3(-0.1f, 0, 0);
                        }

                        break;
                    }
                    case 2:
                        handle1Prop.vector3Value = Vector3.zero;
                        handle2Prop.vector3Value = Vector3.zero;
                        break;
                }
            }

            Vector3 newPointPos = EditorGUILayout.Vector3Field("Position : ", point.transform.localPosition);
            if (newPointPos != point.transform.localPosition) Undo.RecordObject(point.transform, "Move Bezier Point");

            switch (handleStyleProp.enumValueIndex)
            {
                case 0: {
                    Vector3 newPosition = EditorGUILayout.Vector3Field("Handle 1", handle1Prop.vector3Value);
                    if (newPosition != handle1Prop.vector3Value)
                    {
                        handle1Prop.vector3Value = newPosition;
                        handle2Prop.vector3Value = -newPosition;
                    }

                    newPosition = EditorGUILayout.Vector3Field("Handle 2", handle2Prop.vector3Value);
                    if (newPosition != handle2Prop.vector3Value)
                    {
                        handle1Prop.vector3Value = -newPosition;
                        handle2Prop.vector3Value = newPosition;
                    }

                    break;
                }
                case 1:
                    EditorGUILayout.PropertyField(handle1Prop);
                    EditorGUILayout.PropertyField(handle2Prop);
                    break;
            }

            EditorGUI.indentLevel--;
            EditorGUI.indentLevel--;

            if (GUI.changed)
            {
                serObj.ApplyModifiedProperties();
                EditorUtility.SetDirty(serObj.targetObject);
            }
        }

        private static void DrawPointSceneGUI(BezierPoint point)
        {
            if (point.GetComponentInParent<Track>()?.IsVanillaSwitch == true)
                return;
            Handles.Label(point.position + new Vector3(0, HandleUtility.GetHandleSize(point.position) * 0.4f, 0), point.gameObject.name);

            Handles.color = Color.green;
            Vector3 newPosition = Handles.FreeMoveHandle(point.position, point.transform.rotation, HandleUtility.GetHandleSize(point.position) * 0.1f, Vector3.zero, Handles.RectangleHandleCap);

            if (newPosition != point.position)
            {
                Undo.RecordObject(point.transform, "Move Point");
                point.transform.position = newPosition;
            }

            if (point.handleStyle == BezierPoint.HandleStyle.None)
                return;

            Handles.color = Color.cyan;
            Vector3 newGlobal1 = Handles.FreeMoveHandle(point.globalHandle1, point.transform.rotation, HandleUtility.GetHandleSize(point.globalHandle1) * 0.075f, Vector3.zero, Handles.CircleHandleCap);
            if (point.globalHandle1 != newGlobal1)
            {
                Undo.RecordObject(point, "Move Handle");
                point.globalHandle1 = newGlobal1;
                if (point.handleStyle == BezierPoint.HandleStyle.Connected) point.globalHandle2 = -(newGlobal1 - point.position) + point.position;
            }

            Vector3 newGlobal2 = Handles.FreeMoveHandle(point.globalHandle2, point.transform.rotation, HandleUtility.GetHandleSize(point.globalHandle2) * 0.075f, Vector3.zero, Handles.CircleHandleCap);
            if (point.globalHandle2 != newGlobal2)
            {
                Undo.RecordObject(point, "Move Handle");
                point.globalHandle2 = newGlobal2;
                if (point.handleStyle == BezierPoint.HandleStyle.Connected) point.globalHandle1 = -(newGlobal2 - point.position) + point.position;
            }

            Handles.color = Color.yellow;
            Handles.DrawLine(point.position, point.globalHandle1);
            Handles.DrawLine(point.position, point.globalHandle2);
        }

        public static void DrawOtherPoints(BezierCurve curve, BezierPoint caller)
        {
            foreach (BezierPoint p in curve.GetAnchorPoints())
                if (p != caller)
                    DrawPointSceneGUI(p);
        }
    }
}
#endif
