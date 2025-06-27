using System;
using System.Collections.Generic;
using DV.OriginShift;
using HarmonyLib;
using Mapify.Components;
using Mapify.Utils;
using UnityEngine;

namespace Mapify.Patches
{
    [HarmonyPatch(typeof(TurntableRailTrack), nameof(TurntableRailTrack.RotateToTargetRotation))]
    public class TurntableRailTrack_RotateToTargetRotation_Patch
    {
        // cursed override
        public static bool Prefix(TurntableRailTrack __instance, bool forceConnectionRefresh)
        {
            if (!(__instance is TransferTableRailTrack transferTableRailTrack)) return true;
            transferTableRailTrack.MoveToTargetPosition(forceConnectionRefresh);
            return false;
        }

        public static void Postfix(TurntableRailTrack __instance)
        {
            RailwayMeshUpdater.UpdateTrack(__instance.Track);
        }
    }

    [HarmonyPatch(typeof(TurntableRailTrack), nameof(TurntableRailTrack.GetTrackEnd))]
    public class TurntableRailTrack_GetTrackEnd_Patch
    {
        public static bool Prefix(TurntableRailTrack __instance, ref TurntableRailTrack.TrackEnd __result, RailTrack rt)
        {
            if (!(__instance is TransferTableRailTrack transferTableRailTrack)) return true;
            __result = transferTableRailTrack.GetTrackEnd(rt);
            return false;
        }
    }
}
