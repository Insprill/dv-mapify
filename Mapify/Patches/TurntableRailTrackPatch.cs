using System.Linq;
using System.Reflection;
using DV.JObjectExtstensions;
using HarmonyLib;
using Mapify.Editor;
using Mapify.Editor.Utils;
using Mapify.Utils;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Mapify.Patches
{
    [HarmonyPatch(typeof(TurntableRailTrack), nameof(TurntableRailTrack.GetStateSaveData))]
    public static class TurntableRailTrack_GetStateSaveData_Patch
    {
        private static bool Prefix(TurntableRailTrack __instance, ref JObject __result)
        {
            Turntable turntable = __instance.GetComponentInParent<Turntable>();
            if (!(turntable is Traverser traverser))
                return true;
            JObject dataObject = new JObject();
            dataObject.SetFloat("rot", Vectors.InverseLerp(traverser.min, traverser.max, traverser.currentPosition));
            __result = dataObject;
            return false;
        }
    }

    [HarmonyPatch(typeof(TurntableRailTrack), nameof(TurntableRailTrack.SetStateSaveData))]
    public static class TurntableRailTrack_SetStateSaveData_Patch
    {
        private static bool Prefix(TurntableRailTrack __instance, JObject saveData)
        {
            Turntable turntable = __instance.GetComponentInParent<Turntable>();
            if (!(turntable is Traverser traverser))
                return true;
            float? nullable = saveData.GetFloat("rot");
            if (!nullable.HasValue)
            {
                traverser.targetPosition = Vectors.InverseLerp(traverser.min, traverser.max, turntable.bridge.position);
                __instance.RotateToTargetRotation();
            }
            else
            {
                traverser.targetPosition = nullable.Value;
                __instance.RotateToTargetRotation();
            }
            Main.Log($"Loading to {traverser.targetPosition}");
            return false;
        }
    }

    [HarmonyPatch(typeof(TurntableRailTrack), nameof(TurntableRailTrack.RotateToTargetRotation))]
    public static class TurntableRailTrack_RotateToTargetRotation_Patch
    {
        private static readonly MethodInfo TurntableRailTrack_Method_UpdateTrackConnection = AccessTools.DeclaredMethod(typeof(TurntableRailTrack), "UpdateTrackConnection");

        private static bool Prefix(TurntableRailTrack __instance)
        {
            Turntable turntable = __instance.GetComponentInParent<Turntable>();
            if (!(turntable is Traverser traverser))
                return true;

            float difference = traverser.targetPosition - traverser.CurrentPos();

            Transform visuals = __instance.visuals;
            Main.Log($"Moving to {traverser.targetPosition} ({traverser.GetPosition(traverser.targetPosition)})");
            visuals.position = traverser.GetPosition(traverser.targetPosition);
            // visuals.Translate(traverser.GetPosition(traverser.targetPosition));

            // todo
            for (int index = 0; index < __instance.Track.curve.pointCount; ++index)
            {
                BezierPoint bezierPoint = __instance.Track.curve[index];
                Transform pointTransform = bezierPoint.transform;
                // pointTransform.position += pointTransform.forward * difference;
            }

            return false;
        }

        private static void Postfix(TurntableRailTrack __instance)
        {
            __instance.Track.RefreshPointSet();
            RailwayMeshUpdater.UpdateTrack(__instance.Track);
            TurntableRailTrack_Method_UpdateTrackConnection.Invoke(__instance, null);
        }
    }

    [HarmonyPatch(typeof(TurntableRailTrack), "GetTrackEnd")]
    public static class TurntableRailTrack_GetTrackEnd_Patch
    {
        private static bool Prefix(TurntableRailTrack __instance, RailTrack rt, ref TurntableRailTrack.TrackEnd __result)
        {
            Turntable turntable = __instance.GetComponentInParent<Turntable>();
            if (!(turntable is Traverser traverser))
                return true;
            if (rt.isJunctionTrack)
            {
                __result = null;
                return false;
            }

            Main.Log($"Checking connection for {rt.name}");
            BoxCollider collider = __instance.Track.GetComponent<BoxCollider>();
            if (InRange(collider, rt.curve[0].position, __instance.SearchRadius))
                __result = new TurntableRailTrack.TrackEnd {
                    isFirst = true,
                    track = rt,
                    angle = GetAngleForTrackEnd(traverser, rt.curve[0].position)
                };
            else if (InRange(collider, rt.curve.Last().position, __instance.SearchRadius))
                __result = new TurntableRailTrack.TrackEnd {
                    isFirst = false,
                    track = rt,
                    angle = GetAngleForTrackEnd(traverser, rt.curve.Last().position)
                };
            if (__result != null)
            {
                Main.Log($"Found connection. Angle: {__result.angle}");
            }
            return false;
        }

        private static bool InRange(BoxCollider collider, Vector3 position, float threshold)
        {
            Vector3 closestPoint = collider.ClosestPositionOnBounds(position);
            float distToSurface = Mathf.Abs(Vector3.Distance(position, closestPoint));
            return distToSurface <= threshold;
        }

        private static float GetAngleForTrackEnd(Traverser traverser, Vector3 trackEndPosition)
        {
            (float angle, int side) = Vectors.GetDistanceAndSide(traverser.min, traverser.max, trackEndPosition);
            return side > 0 ? angle : -angle;
        }
    }


    [HarmonyPatch(typeof(TurntableRailTrack), nameof(TurntableRailTrack.ClosestSnappingAngle))]
    public static class TurntableRailTrack_ClosestSnappingAngle_Patch
    {
        private static bool Prefix(TurntableRailTrack __instance, ref float __result)
        {
            Turntable turntable = __instance.GetComponentInParent<Turntable>();
            if (!(turntable is Traverser traverser))
                return true;
            __result = __instance.trackEnds.Min(end => traverser.CurrentPos() - end.angle);
            return false;
        }
    }

    [HarmonyPatch(typeof(TurntableRailTrack), "GetConnectedTrackEndOnAngle")]
    public static class TurntableRailTrack_GetConnectedTrackEndOnAngle_Patch
    {
        private static bool Prefix(TurntableRailTrack __instance, float angle, ref TurntableRailTrack.TrackEnd __result)
        {
            Turntable turntable = __instance.GetComponentInParent<Turntable>();
            if (!(turntable is Traverser traverser))
                return true;
            foreach (TurntableRailTrack.TrackEnd end in __instance.trackEnds)
                if (TurntableRailTrack.AnglesEqual(end.angle, angle, 0.03f))
                    __result = end;

            return false;
        }
    }
}
