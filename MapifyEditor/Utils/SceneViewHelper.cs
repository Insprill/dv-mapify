using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace Mapify.Editor.Utils
{
    public static class SceneViewHelper
    {
        public static Vector3 GetMousePosition(SceneView scene, Event e)
        {
            Vector3 mousePos = e.mousePosition;
            mousePos *= EditorGUIUtility.pixelsPerPoint;
            mousePos.y = scene.camera.pixelHeight - mousePos.y;
            return mousePos;
        }
    }
}
#endif
