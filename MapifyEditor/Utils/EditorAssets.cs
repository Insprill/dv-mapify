using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Mapify.Editor.Utils
{
    public static class EditorAssets
    {
        public static T[] FindAllAssets<T>() where T : Object
        {
            return AssetDatabase.FindAssets($"t:{typeof(T).Name}").Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<T>).ToArray();
        }
    }
}
