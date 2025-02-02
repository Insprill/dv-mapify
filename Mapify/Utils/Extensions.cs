using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CommandTerminal;
using DV.JObjectExtstensions;
using DV.PointSet;
using DV.ThingTypes;
using HarmonyLib;
using Mapify.Editor;
using Mapify.Editor.Utils;
using Mapify.Map;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Mapify.Utils
{
    public static class Extensions
    {
        private const string SAVE_KEY_NAME = "mapify";

        #region GameObjects & Components

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

        public static GameObject Replace(this GameObject gameObject, GameObject other, Type[] preserveTypes = null, bool keepChildren = true, Vector3 rotationOffset = default)
        {
            if (gameObject == other) throw new ArgumentException("Cannot replace self with self");
            Transform thisTransform = gameObject.transform;
            Transform otherTransform = other.transform;
            otherTransform.SetParent(thisTransform.parent);
            otherTransform.SetPositionAndRotation(thisTransform.position, thisTransform.rotation);
            otherTransform.Rotate(rotationOffset);
            otherTransform.SetSiblingIndex(thisTransform.GetSiblingIndex());
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
                foreach (Transform child in thisTransform.GetChildren())
                    child.SetParent(otherTransform);

            GameObject.DestroyImmediate(gameObject);
            return other;
        }

        public static void SetActive(this IEnumerable<Component> components, bool active)
        {
            foreach (Component component in components)
                component.gameObject.SetActive(active);
        }

        public static void PrintHierarchy(this GameObject gameObject, string indent = "")
        {
            Transform t = gameObject.transform;
            Mapify.Log($"{indent}+-- {t.name}");
            foreach (Component component in t.GetComponents<Component>()) Mapify.Log($"{indent}|   +-- {component.GetType().Name}");
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

        public static bool Eq(this float f1, float f2, float tolerance = 0.001f)
        {
            return Math.Abs(f1 - f2) < tolerance;
        }

        public static To ConvertByName<From, To>(this From value) where From : Enum where To : Enum
        {
            return (To)Enum.Parse(typeof(To), value.ToString());
        }

        public static List<To> ConvertByName<From, To>(this IEnumerable<From> values) where From : Enum where To : Enum
        {
            return values.Select(c => c.ConvertByName<From, To>()).ToList();
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
        private static readonly MethodInfo DisplayLoadingInfo_Method_OnLoadingStatusChanged =
            AccessTools.DeclaredMethod(typeof(DisplayLoadingInfo), "OnLoadingStatusChanged", new[] { typeof(string), typeof(bool), typeof(float) });

        public static double Double(this CommandArg arg)
        {
            if (double.TryParse(arg.String, out double result))
                return result;
            CommandArg_Method_TypeError.Invoke(arg, new object[] { "double" });
            return 0;
        }

        public static void UpdateLoadingStatus(this DisplayLoadingInfo loadingInfo, string message, float percentLoaded)
        {
            DisplayLoadingInfo_Method_OnLoadingStatusChanged.Invoke(loadingInfo, new object[] { message, false, percentLoaded });
        }

        public static IEnumerable<Vector2> GetCurvePositions(this RailTrack track, float resolution)
        {
            EquiPointSet pointSet = track.GetPointSet();
            EquiPointSet simplified = EquiPointSet.ResampleEquidistant(pointSet, Mathf.Min(resolution, (float)pointSet.span / 3));

            foreach (EquiPointSet.Point point in simplified.points)
                yield return new Vector2((float)point.position.x, (float)point.position.z);
        }

        public static BasicMapInfo GetBasicMapInfo(this SaveGameManager saveGameManager)
        {
            JObject mapify = saveGameManager.data.GetJObject(SAVE_KEY_NAME);
            return mapify != null ? mapify.ToObject<JObject>().ToObject<BasicMapInfo>() : Maps.DEFAULT_MAP_INFO;
        }

        public static BasicMapInfo GetBasicMapInfo(this JObject jObject)
        {
            JObject mapify = jObject.GetJObject(SAVE_KEY_NAME);
            return mapify != null ? mapify.ToObject<JObject>().ToObject<BasicMapInfo>() : Maps.DEFAULT_MAP_INFO;
        }

        public static void SetBasicMapInfo(this JObject jObject, BasicMapInfo basicMapInfo)
        {
            if (basicMapInfo.IsDefault())
                jObject.Remove(SAVE_KEY_NAME);
            else
                jObject.SetJObject(SAVE_KEY_NAME, JObject.FromObject(basicMapInfo));
        }

        public static void SetBasicMapInfo(this SaveGameData saveGameData, BasicMapInfo basicMapInfo)
        {
            if (basicMapInfo.IsDefault())
                saveGameData.RemoveData(SAVE_KEY_NAME);
            else
                saveGameData.SetJObject(SAVE_KEY_NAME, JObject.FromObject(basicMapInfo));
        }

        public static RailTrack GetRailTrack(this RailTrackRegistry registry, string stationID, string yardID, byte trackNumber)
        {
            var query = $"[{stationID}]_[{yardID}-{trackNumber:D2}";

            return registry.AllTracks.FirstOrDefault(track => track.name.Contains(query));
        }

        /// <summary>
        /// Returns the subyard IDs of all subyards in the yard(station) with ID yardID
        /// </summary>
        public static IEnumerable<string> GetSubYardIDsOfYard(this RailTrackRegistry registry, string yardID)
        {
            return registry.AllTracks
                .Select(railTrack => railTrack.logicTrack.ID)
                .Where(ID => ID.yardId == yardID)
                .Select(ID => ID.subYardId)
                .Distinct();
        }

        /// <summary>
        /// Returns the track numbers of all track in the subyard with ID subYardID in the yard(station) with yardID
        /// </summary>
        public static IEnumerable<int> GetTrackNumbersOfSubYard(this RailTrackRegistry registry, string yardID, string subYardID)
        {
            return registry.AllTracks
                .Select(railTrack => railTrack.logicTrack.ID)
                .Where(ID => ID.yardId == yardID &&
                             ID.subYardId == subYardID)
                .Select(ID => ID.orderNumber)
                .Where(trackNumberString => trackNumberString != "")
                .Select(int.Parse)
                .Distinct();
        }

        public static void SwitchTo(this Junction junction, byte branchNumber, Junction.SwitchMode switchMode)
        {
            Mapify.LogDebug($"junction {junction.name} switch to {branchNumber}");

            junction.selectedBranch = (byte) Misc.BetterModulo(branchNumber - 1, junction.outBranches.Count);
            junction.Switch(switchMode);
        }

        public static Junction.Branch FindClosestBranch(this Junction junction, Vector3 fromPoint, float maxRange = 5f)
        {
            var closestDistance = float.PositiveInfinity;

            RailTrack track = null;
            var first = false;

            foreach (var foundTrack in Resources.FindObjectsOfTypeAll<RailTrack>())
            {
                // skip the tracks in the junction
                if(junction.outBranches.Any(branch => branch.track == foundTrack)) continue;

                if (!foundTrack.curve || foundTrack.curve.pointCount < 2) continue;

                var firstPoint = foundTrack.curve[0];

                var distanceToFirst = Vector3.SqrMagnitude(fromPoint - firstPoint.position);
                if (distanceToFirst < maxRange * (double) maxRange && distanceToFirst < (double) closestDistance)
                {
                    closestDistance = distanceToFirst;
                    track = foundTrack;
                    first = true;
                }

                var lastPoint = foundTrack.curve.Last();
                var distanceToLast = Vector3.SqrMagnitude(fromPoint - lastPoint.position);
                if (distanceToLast < maxRange * (double) maxRange && distanceToLast < (double) closestDistance)
                {
                    closestDistance = distanceToLast;
                    track = foundTrack;
                    first = false;
                }
            }

            if (track == null)
            {
                Mapify.LogError($"Failed to find closest branch for {junction.name}");
                return null;
            }

            return new Junction.Branch(track, first);
        }

        #endregion

        #region Mapify

        public static void Replace(this IEnumerable<VanillaObject> vanillaObjects, bool active = true, bool originShift = true, Type[] preserveTypes = null)
        {
            foreach (VanillaObject vanillaObject in vanillaObjects) vanillaObject.Replace(active, originShift, preserveTypes);
        }

        public static GameObject Replace(this VanillaObject vanillaObject, bool active = true, bool originShift = true, Type[] preserveTypes = null)
        {
            return vanillaObject.gameObject.Replace(AssetCopier.Instantiate(vanillaObject.asset, active, originShift), preserveTypes, vanillaObject.keepChildren, vanillaObject.rotationOffset);
        }

        #endregion
    }
}
