#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Mapify.Editor.Utils
{
    public static class EditorAssets
    {
        public static T[] FindAssets<T>() where T : Object
        {
            return AssetDatabase.FindAssets($"t:{typeof(T).Name}").Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<T>).ToArray();
        }

        public static T FindAsset<T>() where T : Object
        {
            return FindAssets<T>().FirstOrDefault();
        }
    }
}
#endif
