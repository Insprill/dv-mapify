using Bolt;
using HarmonyLib;
using Mapify.Components;
using UnityEngine;

namespace Mapify.Patches
{
    /// <summary>
    /// use TracksUpdated2 because we cant access TracksUpdated from within TransferTableRailTrack
    /// </summary>
    [HarmonyPatch(typeof(OperateTurntableUnit), nameof(OperateTurntableUnit.PrepareContext))]
    public class OperateTurntableUnit_PrepareContext_Patch
    {
        public static bool Prefix(OperateTurntableUnit __instance, ref object __result, Flow flow)
        {
            var context = new OperateTurntableUnit.Context
            {
                TrackReference = flow.GetValue<GameObject>(__instance.trackReferenceObject),
                TurnTableFinder = flow.GetValue<GameObject>(__instance.turntableFinderObject).GetComponent<TutorialTurnTableFinder>()
            };
            context.TurnTableFinder.Initialize();
            var controller = context.TurnTableFinder.controller;

            if (context.TrackReference == null)
            {
                if (controller is TransferTableController transfertableController)
                {
                    transfertableController.Snapped2 += context.OnSnapped;
                }
                else
                {
                    controller.Snapped += context.OnSnapped;
                }
            }
            else
            {
                context.OffTurntableTrack = CarSpawner.GetTrackClosestTo(context.TrackReference.transform.position, 1f, out _);

                var turntableTrack = controller.turntable;
                if (turntableTrack is TransferTableRailTrack transfertableTrack)
                {
                    transfertableTrack.TracksUpdated2 += context.OnTracksUpdated;
                }
                else
                {
                    turntableTrack.TracksUpdated += context.OnTracksUpdated;
                }
            }
            __result = context;

            return false;
        }
    }


    /// <summary>
    /// use TracksUpdated2 because we cant access TracksUpdated from within TransferTableRailTrack
    /// </summary>
    [HarmonyPatch(typeof(OperateTurntableUnit), nameof(OperateTurntableUnit.CleanupContext))]
    public class OperateTurntableUnit_CleanupContext_Patch
    {
        public static bool Prefix(OperateTurntableUnit __instance, Flow flow, object context)
        {
            var context1 = (OperateTurntableUnit.Context) context;
            var controller = context1.TurnTableFinder.controller;

            if (context1.TrackReference == null)
            {
                if (controller is TransferTableController transfertableController)
                {
                    transfertableController.Snapped2 -= context1.OnSnapped;
                }
                else
                {
                    controller.Snapped -= context1.OnSnapped;
                }
            }
            else
            {
                var turntableTrack = controller.turntable;
                if (turntableTrack is TransferTableRailTrack transfertableTrack)
                {
                    transfertableTrack.TracksUpdated2 -= context1.OnTracksUpdated;
                }
                else {
                    turntableTrack.TracksUpdated -= context1.OnTracksUpdated;
                }
            }

            return false;
        }
    }
}
