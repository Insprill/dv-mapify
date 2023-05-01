using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using CommandTerminal;
using DV.Logic.Job;
using DV.PointSet;
using HarmonyLib;
using Mapify.Editor;
using Mapify.Editor.Utils;
using UnityEngine;

namespace Mapify.Utils
{
    public static class Extensions
    {
        #region GameObjects & Components

        public static GameObject NewChild(this WorldMover worldMover, string name)
        {
            return worldMover.originShiftParent.gameObject.NewChild(name);
        }

        public static GameObject NewChildWithPosition(this WorldMover worldMover, string name, Vector3 position)
        {
            return worldMover.originShiftParent.gameObject.NewChildWithPosition(name, position);
        }

        public static GameObject NewChild(this GameObject parent, string name)
        {
            return NewChildWithPosition(parent, name, Vector3.zero);
        }

        public static GameObject NewChildWithPosition(this GameObject parent, string name, Vector3 position)
        {
            return new GameObject(name) {
                transform = {
                    parent = parent.transform,
                    position = position
                }
            };
        }

        public static T WithComponent<T>(this GameObject gameObject) where T : Component
        {
            T comp = gameObject.GetComponent<T>();
            return comp ? comp : gameObject.AddComponent<T>();
        }

        public static GameObject Replace(this GameObject gameObject, GameObject other, Type[] preserveTypes = null, bool keepChildren = true)
        {
            if (gameObject == other) throw new ArgumentException("Cannot replace self with self");
            Transform t = gameObject.transform;
            Transform ot = other.transform;
            ot.SetParent(t.parent);
            ot.SetPositionAndRotation(t.position, t.rotation);
            ot.SetSiblingIndex(t.GetSiblingIndex());
            if (preserveTypes != null)
                foreach (Type type in preserveTypes)
                {
                    Component[] components = gameObject.GetComponents(type);
                    if (components.Length == 0) continue;
                    FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    foreach (Component component in components)
                    {
                        Component newComponent = other.AddComponent(type);
                        foreach (FieldInfo field in fields)
                            field.SetValue(newComponent, field.GetValue(component));
                    }
                }

            if (keepChildren)
                foreach (Transform child in t.GetChildren())
                    child.SetParent(ot);

            GameObject.DestroyImmediate(gameObject);
            return other;
        }

        public static void PrintHierarchy(this GameObject gameObject, string indent = "")
        {
            Transform t = gameObject.transform;
            Main.Log($"{indent}+-- {t.name}");
            foreach (Component component in t.GetComponents<Component>()) Main.Log($"{indent}|   +-- {component.GetType().Name}");
            foreach (Transform child in t) PrintHierarchy(child.gameObject, $"{indent}|   ");
        }

        #endregion

        #region Misc. Unity Types

        public static Vector3 AddY(this Vector3 vec, float y)
        {
            vec.y += y;
            return vec;
        }

        public static Vector3 Add(this Vector3 vec, float val)
        {
            vec.x += val;
            vec.y += val;
            vec.z += val;
            return vec;
        }

        public static Vector2 Scale(this Vector2 vec, float minValue, float maxValue, float newMinValue, float newMaxValue)
        {
            return new Vector2(vec.x.ScaleNumber(minValue, maxValue, newMinValue, newMaxValue), vec.y.ScaleNumber(minValue, maxValue, newMinValue, newMaxValue));
        }

        public static Vector2 ToXZ(this Vector3 vec)
        {
            return new Vector2(vec.x, vec.z);
        }

        public static Vector3 SwapAndInvertXZ(this Vector3 vec)
        {
            return new Vector3(-vec.z, vec.y, -vec.x);
        }

        #endregion

        #region Mapify -> Vanilla Converters

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

        public static To ConvertByName<From, To>(this From value) where From : Enum where To : Enum
        {
            return (To)Enum.Parse(typeof(To), value.ToString());
        }

        public static List<To> ConvertByName<From, To>(this IEnumerable<From> values) where From : Enum where To : Enum
        {
            return values.Select(c => c.ConvertByName<From, To>()).ToList();
        }

        public static string ToSpacedString<TEnum>(this TEnum value) where TEnum : Enum
        {
            string stringValue = value.ToString();
            StringBuilder spacedStringBuilder = new StringBuilder(stringValue.Length);
            char lastChar = char.MinValue;

            foreach (char c in stringValue)
            {
                if (lastChar != char.MinValue && ((char.IsUpper(c) && !char.IsUpper(lastChar)) || (char.IsNumber(c) && !char.IsNumber(lastChar))))
                    spacedStringBuilder.Append(' ');

                spacedStringBuilder.Append(c);
                lastChar = c;
            }

            return spacedStringBuilder.ToString();
        }

        public static float ScaleNumber(this float value, float minValue, float maxValue, float newMinValue, float newMaxValue)
        {
            float percentage = (value - minValue) / (maxValue - minValue);
            float newValue = (newMaxValue - newMinValue) * percentage + newMinValue;
            return newValue;
        }

        #endregion

        #region DV

        private static readonly MethodInfo CommandArg_Method_TypeError = AccessTools.DeclaredMethod(typeof(CommandArg), "TypeError", new[] { typeof(string) });
        private static readonly MethodInfo WorldStreamingInit_Method_Info = AccessTools.DeclaredMethod(typeof(WorldStreamingInit), "Info", new[] { typeof(string), typeof(float) });

        public static double Double(this CommandArg arg)
        {
            if (double.TryParse(arg.String, out double result))
                return result;
            CommandArg_Method_TypeError.Invoke(arg, new object[] { "double" });
            return 0;
        }

        public static void Log(this WorldStreamingInit wsi, string message, float percentLoaded)
        {
            WorldStreamingInit_Method_Info.Invoke(wsi, new object[] { message, percentLoaded });
        }

        public static IEnumerable<Vector2> GetCurvePositions(this RailTrack track, float resolution)
        {
            EquiPointSet pointSet = track.GetPointSet();
            EquiPointSet simplified = EquiPointSet.ResampleEquidistant(pointSet, Mathf.Min(resolution, (float)pointSet.span / 3));

            foreach (EquiPointSet.Point point in simplified.points)
                yield return new Vector2((float)point.position.x, (float)point.position.z);
        }

        #endregion

        #region Mapify

        public static void Replace(this IEnumerable<VanillaObject> vanillaObjects, bool active = true, bool keepChildren = true, bool originShift = true, Type[] preserveTypes = null)
        {
            foreach (VanillaObject vanillaObject in vanillaObjects) vanillaObject.Replace(active, keepChildren, originShift, preserveTypes);
        }

        public static GameObject Replace(this VanillaObject vanillaObject, bool active = true, bool keepChildren = true, bool originShift = true, Type[] preserveTypes = null)
        {
            return vanillaObject.gameObject.Replace(AssetCopier.Instantiate(vanillaObject.asset, originShift, active), preserveTypes, keepChildren);
        }

        #endregion
    }
}
