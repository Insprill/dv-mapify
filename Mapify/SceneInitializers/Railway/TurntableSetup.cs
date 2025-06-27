using System;
using System.Reflection;
using HarmonyLib;
using Mapify.Components;
using Mapify.Editor;
using Mapify.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Mapify.SceneInitializers.Railway
{
    public class TurntableSetup : SceneSetup
    {
        public override void Run()
        {
            Mapify.LogDebug(() => "Creating turntables");
            foreach (Turntable turntable in Object.FindObjectsOfType<Turntable>())
            {
                (TurntableController controller, bool usingDefaultTrack) = SetupVanillaObjects(turntable);
                var turntableTrack = SetupTurntableTrack(turntable, usingDefaultTrack);
                SetupTurntableController(controller, turntableTrack);
            }
        }

        private static Tuple<TurntableController, bool> SetupVanillaObjects(Turntable turntable)
        {
            TurntableController controller = null;
            bool usingDefaultTrack = false;
            foreach (VanillaObject vanillaObject in turntable.GetComponentsInChildren<VanillaObject>())
            {
                if (vanillaObject.asset == VanillaAsset.TurntableTrack)
                    usingDefaultTrack = true;
                switch (vanillaObject.asset)
                {
                    case VanillaAsset.TurntablePit:
                    case VanillaAsset.TurntableTrack:
                    case VanillaAsset.TurntableBridge:
                    case VanillaAsset.TurntableControlShed:
                        vanillaObject.Replace();
                        break;
                    case VanillaAsset.TurntableControlPanel:
                        var originalObj = vanillaObject.Replace(false);
                        var originalController = originalObj.GetComponent<TurntableController>();

                        if (turntable is TransferTable _)
                        {
                            var temp = originalObj.AddComponent<TransferTableController>();
                            temp.CopyValues(originalController);
                            Object.Destroy(originalController);
                            controller = temp;
                        }
                        else
                        {
                            controller = originalController;
                        }
                        break;
                }
            }

            return new Tuple<TurntableController, bool>(controller, usingDefaultTrack);
        }

        private static TurntableRailTrack SetupTurntableTrack(Turntable turntable, bool usingDefaultTrack)
        {
            Track track = turntable.Track;
            TurntableRailTrack turntableTrack;

            if (turntable is TransferTable transferTable)
            {
                var temp = track.gameObject.AddComponent<TransferTableRailTrack>();
                temp.TransferTableWidth = transferTable.Pit.GetComponent<MeshRenderer>().bounds.size.x; //todo assumption
                turntableTrack = temp;
            }
            else
            {
                turntableTrack = track.gameObject.AddComponent<TurntableRailTrack>();
            }

            RailTrack railTrack = track.GetComponent<RailTrack>();
            railTrack.generateMeshes = !usingDefaultTrack;
            turntableTrack._track = railTrack;
            turntableTrack.uniqueID = $"{turntableTrack.transform.position.GetHashCode()}";
            turntableTrack.trackEnds = turntableTrack.FindTrackEnds();
            turntableTrack.visuals = turntable.bridge;
            if (turntable.frontHandle != null)
                turntableTrack.frontHandle = turntable.frontHandle.transform;
            if (turntable.rearHandle != null)
                turntableTrack.rearHandle = turntable.rearHandle.transform;
            return turntableTrack;
        }

        private static void SetupTurntableController(TurntableController controller, TurntableRailTrack turntableTrack)
        {
            controller.turntableRotateLayered = AssetCopier.Instantiate(VanillaAsset.TurntableRotateLayered).GetComponent<LayeredAudio>();
            controller.turntable = turntableTrack;
            controller.gameObject.SetActive(true);
        }
    }
}
