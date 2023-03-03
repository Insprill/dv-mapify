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
    }
}
