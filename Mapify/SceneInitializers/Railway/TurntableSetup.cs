using System;
using System.Reflection;
using HarmonyLib;
using Mapify.Editor;
using Mapify.Utils;
using Object = UnityEngine.Object;

namespace Mapify.SceneInitializers.Railway
{
    public class TurntableSetup : SceneSetup
    {
        private static readonly FieldInfo TurntableRailTrack_Field__track = AccessTools.DeclaredField(typeof(TurntableRailTrack), "_track");

        public override void Run()
        {
            Mapify.LogDebug("Creating turntables");
            foreach (Turntable turntable in Object.FindObjectsOfType<Turntable>())
            {
                (TurntableController controller, bool usingDefaultTrack) = SetupVanillaObjects(turntable);
                TurntableRailTrack turntableTrack = SetupTurntableTrack(turntable, usingDefaultTrack);
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
                        controller = vanillaObject.Replace(false).GetComponent<TurntableController>();
                        break;
                }
            }

            return new Tuple<TurntableController, bool>(controller, usingDefaultTrack);
        }

        private static TurntableRailTrack SetupTurntableTrack(Turntable turntable, bool usingDefaultTrack)
        {
            Track track = turntable.Track;
            TurntableRailTrack turntableTrack = track.gameObject.AddComponent<TurntableRailTrack>();
            RailTrack railTrack = track.GetComponent<RailTrack>();
            railTrack.generateMeshes = !usingDefaultTrack;
            TurntableRailTrack_Field__track.SetValue(turntableTrack, railTrack);
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
