using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Mapify.Editor.Utils
{
    public static class Extensions
    {
        #region GameObjects & Components

        public static T GetComponentInSelfOrParent<T>(this Component component)
        {
            T self = component.GetComponent<T>();
            return self != null ? self : component.GetComponentInParent<T>();
        }

        public static T[] GetFirstComponentInChildren<T>(this GameObject gameObject, bool includeInactive = false) where T : Component
        {
            return gameObject.GetComponentsInChildren<T>(includeInactive)
                .GroupBy(c => c.gameObject)
                .Select(g => g.First())
                .ToArray();
        }

        public static T GetClosestComponent<T>(this Vector3 position) where T : Component
        {
            return Object.FindObjectsOfType<T>()
                .OrderBy(c => (position - c.transform.position).sqrMagnitude)
                .FirstOrDefault();
        }

        private static T GetClosestComponent<T>(this GameObject gameObject) where T : Component
        {
            return gameObject.transform.position.GetClosestComponent<T>();
        }

        public static GameObject FindChildByName(this GameObject parent, string name)
        {
            Transform child = FindChildByName(parent.transform, name);
            return child == null ? null : child.gameObject;
        }

        public static GameObject[] FindChildrenByName(this GameObject parent, string name)
        {
            return parent.transform.FindChildrenByName(name).Select(t => t.gameObject).ToArray();
        }

        public static Transform FindChildByName(this Transform parent, string name)
        {
            return FindChildrenByName(parent, name).FirstOrDefault();
        }

        public static Transform[] FindChildrenByName(this Transform parent, string name)
        {
            return parent.GetComponentsInChildren<Transform>(true).Where(t => t.name == name).ToArray();
        }

        public static Transform[] GetChildren(this Transform parent)
        {
            Transform[] children = new Transform[parent.childCount];
            for (int i = 0; i < parent.childCount; i++) children[i] = parent.GetChild(i);
            return children;
        }

        public static T[] GetAllComponents<T>(this Scene scene, bool includeInactive = false)
        {
            return scene.GetRootGameObjects().SelectMany(go => go.GetComponentsInChildren<T>(includeInactive)).ToArray();
        }

        public static GameObject[] GetAllGameObjects(this Scene scene)
        {
            return scene.GetAllComponents<Transform>().Select(t => t.gameObject).ToArray();
        }

        public static void RecordObjectChanges(this IEnumerable<Object> objects, Action func)
        {
            Object[] nonNullObjects = objects.Where(obj => obj != null).ToArray();
            Undo.IncrementCurrentGroup();
            Undo.RecordObjects(nonNullObjects, "Object Changes");

            func.Invoke();

            foreach (Object o in nonNullObjects.Where(PrefabUtility.IsPartOfPrefabInstance))
                PrefabUtility.RecordPrefabInstancePropertyModifications(o);

            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
            EditorSceneManager.SaveOpenScenes();
        }

        public static float CalculateWorldSize(this IEnumerable<Terrain> terrains)
        {
            float maxX = 0f;
            float maxZ = 0f;

            foreach (Terrain terrain in terrains)
            {
                Vector3 terrainSize = terrain.terrainData.size;
                Vector3 position = terrain.transform.position;
                float terrainMaxX = position.x + terrainSize.x;
                float terrainMaxZ = position.z + terrainSize.z;
                if (terrainMaxX > maxX) maxX = terrainMaxX;
                if (terrainMaxZ > maxZ) maxZ = terrainMaxZ;
            }

            return Mathf.Max(maxX, maxZ);
        }

        #endregion

        #region Misc. Unity Types

        public static Terrain[] Sort(this Terrain[] terrains)
        {
            return terrains.OrderBy(go =>
            {
                Vector3 pos = go.transform.position;
                return pos.z * terrains.Length + pos.x;
            }).ToArray();
        }

        public static (float, float, float, float) GroupedBounds(this IEnumerable<Renderer> renderers)
        {
            Bounds[] allBounds = renderers.Select(r => r.bounds).ToArray();
            float minX = allBounds.Min(b => b.min.x);
            float minZ = allBounds.Min(b => b.min.z);
            float maxX = allBounds.Max(b => b.max.x);
            float maxZ = allBounds.Max(b => b.max.z);
            return (minX, minZ, maxX, maxZ);
        }

        #endregion

        #region Bezier Curves

        public static BezierCurve Curve(this BezierPoint point)
        {
            if (point._curve != null) return point._curve;
            BezierCurve curve = point.GetComponentInParent<BezierCurve>();
            point._curve = curve;
            return curve;
        }

        public static BezierPoint[] GetFirstAndLastPoints(this BezierCurve curve)
        {
            return new[] { curve[0], curve.Last() };
        }

        #endregion

        #region C# Utils

        public static List<T> ToList<T>(this IEnumerator<T> e)
        {
            List<T> list = new List<T>();
            while (e.MoveNext()) list.Add(e.Current);
            return list;
        }

        #endregion

        #region Mapify

        public static string PrettySceneName(this string sceneName)
        {
            return sceneName.Split('/').Last().Split('.').First();
        }

        public static Dictionary<Station, List<T>> MapToClosestStation<T>(this IEnumerable<T> arr) where T : Component
        {
            return arr
                .GroupBy(spawner => spawner.gameObject.GetClosestComponent<Station>())
                .Where(group => group.Key != null)
                .ToDictionary(group => group.Key, group => group.ToList());
        }

        #endregion
    }
}
