using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

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
    }
}
