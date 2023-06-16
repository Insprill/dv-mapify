#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Mapify.Editor
{
    public class ObjectReplacer : EditorWindow
    {
        private bool openPicker;

        [MenuItem("GameObject/Replace With Prefab", false, 0)]
        private static void ReplaceWithPrefab(MenuCommand command)
        {
            ObjectReplacer window = GetWindow<ObjectReplacer>();
            window.openPicker = true;
            window.maxSize = Vector2.zero;
            window.Show();
        }

        private void OnGUI()
        {
            if (openPicker)
            {
                int controlId = GUIUtility.GetControlID(FocusType.Passive);
                EditorGUIUtility.ShowObjectPicker<GameObject>(null, false, "", controlId);
                openPicker = false;
            }

            if (Event.current.commandName != "ObjectSelectorClosed")
                return;

            GameObject prefab = EditorGUIUtility.GetObjectPickerObject() as GameObject;
            if (prefab == null)
                return;

            Undo.IncrementCurrentGroup();

            foreach (GameObject toReplace in Selection.gameObjects)
            {
                Transform replaceTransform = toReplace.transform;

                GameObject newObj = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                if (newObj == null)
                    return;
                Undo.RegisterCreatedObjectUndo(newObj, "Instantiate prefab");

                Transform newTransform = newObj.transform;
                newTransform.SetParent(replaceTransform.parent);
                newTransform.SetPositionAndRotation(replaceTransform.position, replaceTransform.rotation);
                newTransform.SetSiblingIndex(replaceTransform.GetSiblingIndex());

                Undo.DestroyObjectImmediate(toReplace);
            }

            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());

            Close();
        }
    }
}
#endif
