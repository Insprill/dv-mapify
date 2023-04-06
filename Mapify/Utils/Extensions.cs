using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CommandTerminal;
using DV.Logic.Job;
using HarmonyLib;
using Mapify.Editor;
using UnityEngine;

namespace Mapify.Utils
{
    public static class Extensions
    {
        #region GameObjects & Components

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

        public static GameObject Replace(this GameObject gameObject, GameObject other, Type[] preserveTypes = null, bool keepChildren = true)
        {
            Transform t = gameObject.transform;
            Transform ot = other.transform;
            ot.SetParent(t.parent, false);
            ot.SetPositionAndRotation(t.position, t.rotation);
            ot.SetSiblingIndex(t.GetSiblingIndex());
            if (preserveTypes != null)
                foreach (Type type in preserveTypes)
                {
                    Component[] components = gameObject.GetComponents(type);
                    foreach (Component component in components)
                    {
                        Component newComponent = other.AddComponent(type);
                        FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        foreach (FieldInfo field in fields)
                            field.SetValue(newComponent, field.GetValue(component));
                    }
                }

            if (keepChildren)
                foreach (Transform child in t)
                    child.SetParent(ot);

            GameObject.DestroyImmediate(gameObject);
            return other;
        }

        public static GameObject FindChildByName(this GameObject parent, string name)
        {
            Transform[] children = parent.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in children)
                if (child.gameObject.name == name)
                    return child.gameObject;
            return null;
        }

        public static void PrintHierarchy(this GameObject gameObject, string indent = "")
        {
            Transform t = gameObject.transform;
            Main.Logger.Log(indent + "+-- " + t.name);

            foreach (Component component in t.GetComponents<Component>()) Main.Logger.Log(indent + "|   +-- " + component.GetType().Name);

            foreach (Transform child in t) PrintHierarchy(child.gameObject, indent + "|   ");
        }

        #endregion

        #region Mapify -> Vanilla Converters

        public static To ConvertByName<From, To>(this From cargo) where From : Enum where To : Enum
        {
            return (To)Enum.Parse(typeof(To), cargo.ToString());
        }

        public static List<To> ConvertByName<From, To>(this IEnumerable<From> cargos) where From : Enum where To : Enum
        {
            return cargos.Select(c => c.ConvertByName<From, To>()).ToList();
        }

        public static List<CargoGroup> ToVanilla(this IEnumerable<CargoSet> list)
        {
            return list?.Select(l =>
                new CargoGroup(
                    l.cargoTypes.ConvertByName<Cargo, CargoType>(),
                    l.stations.Select(s => s.GetComponent<StationController>()).ToList()
                )
            ).ToList();
        }

        #endregion

        #region C# Utils

        public static bool TryAdd<K, V>(this Dictionary<K, V> dictionary, K key, V value)
        {
            bool contains = dictionary.ContainsKey(key);
            if (!contains) dictionary.Add(key, value);
            return !contains;
        }

        #endregion

        #region DV

        private static readonly MethodInfo CommandArg_Method_TypeError = AccessTools.DeclaredMethod(typeof(CommandArg), "TypeError", new[] { typeof(string) });

        public static double Double(this CommandArg arg)
        {
            if (double.TryParse(arg.String, out double result))
                return result;
            CommandArg_Method_TypeError.Invoke(arg, new object[] { "double" });
            return 0;
        }

        #endregion
    }
}
