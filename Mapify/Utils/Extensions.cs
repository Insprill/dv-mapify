using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Mapify.Utils
{
    public static class Extensions
    {
        public static GameObject NewChild(this GameObject parent, string name)
        {
            return NewChildWithTransform(parent, name, Vector3.zero, Vector3.zero);
        }

        public static GameObject NewChildWithPosition(this GameObject parent, string name, Vector3 position)
        {
            return NewChildWithTransform(parent, name, position, Vector3.zero);
        }

        public static GameObject NewChildWithRotation(this GameObject parent, string name, Vector3 rotation)
        {
            return NewChildWithTransform(parent, name, Vector3.zero, rotation);
        }

        public static GameObject NewChildWithTransform(this GameObject parent, string name, Vector3 position, Vector3 rotation)
        {
            return new GameObject(name) {
                transform = {
                    parent = parent.transform,
                    localPosition = position,
                    localEulerAngles = rotation
                }
            };
        }

        public static T WithComponentT<T>(this GameObject gameObject) where T : Component
        {
            T comp = gameObject.GetComponent<T>();
            return comp ? comp : gameObject.AddComponent<T>();
        }

        public static GameObject WithComponent<T>(this GameObject gameObject) where T : Component
        {
            if (!gameObject.GetComponent<T>()) gameObject.AddComponent<T>();
            return gameObject;
        }

        public static IEnumerable<string> ToNames(this IEnumerable<Component> components)
        {
            return components.Select(component => component.gameObject.name);
        }

        public static void PrintHierarchy(this GameObject gameObject, string indent = "")
        {
            Transform t = gameObject.transform;
            Main.Logger.Log(indent + "+-- " + t.name);

            foreach (Component component in t.GetComponents<Component>()) Main.Logger.Log(indent + "|   +-- " + component.GetType().Name);

            foreach (Transform child in t) PrintHierarchy(child.gameObject, indent + "|   ");
        }
    }
}
