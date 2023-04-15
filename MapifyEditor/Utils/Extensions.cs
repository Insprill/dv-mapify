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
        public static void Select(this GameObject gameObject)
        {
            Selection.objects = new Object[] { gameObject };
        }

        public static List<T> ToList<T>(this IEnumerator<T> e)
        {
            List<T> list = new List<T>();
            while (e.MoveNext()) list.Add(e.Current);
            return list;
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

        public static Terrain[] Sort(this Terrain[] terrains)
        {
            return terrains.OrderBy(go =>
            {
                Vector3 pos = go.transform.position;
                return pos.z * terrains.Length + pos.x;
            }).ToArray();
        }

        public static T GetComponentInSelfOrParent<T>(this Component component)
        {
            return component.gameObject.GetComponentInSelfOrParent<T>();
        }

        public static T GetComponentInSelfOrParent<T>(this GameObject gameObject)
        {
            T self = gameObject.GetComponent<T>();
            return self != null ? self : gameObject.GetComponentInParent<T>();
        }

        public static T[] GetFirstComponentInChildren<T>(this GameObject gameObject, bool includeInactive = false) where T : Component
        {
            return gameObject.GetComponentsInChildren<T>(includeInactive)
                .GroupBy(c => c.gameObject)
                .Select(g => g.First())
                .ToArray();
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

        public static Dictionary<Station, List<T>> MapToClosestStation<T>(this IEnumerable<T> arr) where T : Component
        {
            return arr
                .GroupBy(spawner => spawner.gameObject.GetClosestComponent<Station>())
                .Where(group => group.Key != null)
                .ToDictionary(group => group.Key, group => group.ToList());
        }

        public static T GetClosestComponent<T>(this GameObject gameObject) where T : Component
        {
            return Object.FindObjectsOfType<T>()
                .OrderBy(c => (gameObject.transform.position - c.transform.position).sqrMagnitude)
                .FirstOrDefault();
        }

        public static GameObject FindChildByName(this GameObject parent, string name)
        {
            Transform child = FindChildByName(parent.transform, name);
            return child == null ? null : child.gameObject;
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

        public static void RunInScene(this string scenePath, Action<Scene> action)
        {
            RunInScene(scenePath, scene =>
            {
                action.Invoke(scene);
                return true;
            }, true);
        }

        public static T RunInScene<T>(this string scenePath, Func<Scene, T> func, T defaultValue = default)
        {
            Scene scene = SceneManager.GetSceneByPath(scenePath);
            if (!scene.IsValid()) throw new ArgumentException($"Failed to find scene {scenePath}");
            bool wasLoaded = scene.isLoaded;
            if (!wasLoaded) EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            T result = func.Invoke(scene);
            if (!wasLoaded)
                SceneManager.UnloadSceneAsync(scene);
            return EqualityComparer<T>.Default.Equals(result, default) ? defaultValue : result;
        }

        public static T RecordObjectChanges<T>(this IEnumerable<Object> objects, Func<T> func)
        {
            Object[] nonNullObjects = objects.Where(obj => obj != null).ToArray();
            Undo.IncrementCurrentGroup();
            Undo.RecordObjects(nonNullObjects, "Map Validation");

            T result = func.Invoke();

            foreach (Object o in nonNullObjects.Where(PrefabUtility.IsPartOfPrefabInstance))
                PrefabUtility.RecordPrefabInstancePropertyModifications(o);

            EditorSceneManager.SaveOpenScenes();
            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());

            return result;
        }

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

        public static Texture2D Resize(this Texture2D source, int width, int height, FilterMode filterMode)
        {
            RenderTexture rt = new RenderTexture(width, height, 0);
            Graphics.Blit(source, rt, new Material(Shader.Find("Hidden/BlitCopy")));
            RenderTexture.active = rt;

            Texture2D result = new Texture2D(width, height) {
                filterMode = filterMode,
                wrapMode = TextureWrapMode.Clamp
            };
            result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            result.Apply();

            RenderTexture.active = null;
            Object.DestroyImmediate(rt);

            return result;
        }
    }
}
