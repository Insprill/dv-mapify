using Mapify.Editor.Utils;
using UnityEngine;
#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
#endif

namespace Mapify.Editor
{
    public abstract class VisualizableMonoBehaviour : MonoBehaviour
    {
        [Header("Editor Visualization")]
        [SerializeField]
        internal GameObject visualPrefab;

        protected void UpdateVisuals<T>(T[] things, Transform reference)
        {
#if UNITY_EDITOR
            if (PrefabStageUtility.GetCurrentPrefabStage() != null || EditorUtility.IsPersistent(gameObject) || visualPrefab == null)
                return;
            StartCoroutine(UpdateVisualsCoroutine(things, reference));
        }

        private IEnumerator UpdateVisualsCoroutine<T>(IReadOnlyCollection<T> things, Transform reference)
        {
            yield return null;
            DestroyVisuals();

            for (int i = 0; i < things.Count; i++)
            {
                GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(visualPrefab);
                go.tag = "EditorOnly";
                PositionThing(reference, go.transform, i);
            }
#endif
        }

        private void DestroyVisuals()
        {
            foreach (Transform child in transform.FindChildrenByName(visualPrefab.name))
                DestroyImmediate(child.gameObject);
        }

        public abstract void PositionThing(Transform reference, Transform toMove, int count);
    }
}
