using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DV.Signs;
using HarmonyLib;
using Mapify.Editor;
using Mapify.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mapify.SceneInitializers
{
    public static class RailwaySceneInitializer
    {
        private static readonly FieldInfo TurntableRailTrack_Field__track = AccessTools.DeclaredField(typeof(TurntableRailTrack), "_track");

        public static void SceneLoaded(Scene scene)
        {
            Transform railwayParent = WorldMover.Instance.NewChild("[railway]").transform;
            foreach (Transform transform in scene.GetRootGameObjects().Select(go => go.transform))
                transform.SetParent(railwayParent);
            SetupRailTracks();
            CreateSigns();
            CreateTrackLODs();
        }

        private static void SetupRailTracks()
        {
            Mapify.Log("Creating RailTracks");

            Track[] tracks = Object.FindObjectsOfType<Track>().Where(t => !t.IsSwitch).ToArray();
            foreach (Track track in tracks)
            {
                int age = 0;
                switch (track.age)
                {
                    case TrackAge.New:
                        age = 0;
                        break;
                    case TrackAge.Medium:
                        age = 50;
                        break;
                    case TrackAge.Old:
                        age = 100;
                        break;
                }

                track.gameObject.SetActive(false);
                RailTrack railTrack = track.gameObject.AddComponent<RailTrack>();
                railTrack.generateColliders = !track.IsTurntable;
                railTrack.dontChange = false;
                railTrack.age = age;
                railTrack.ApplyRailType();
            }

            Mapify.LogDebug("Creating Junctions");
            foreach (Switch sw in Object.FindObjectsOfType<Switch>())
                CreateJunction(sw);

            Mapify.LogDebug("Connecting tracks");
            ConnectTracks(tracks);

            foreach (Track track in tracks)
                track.gameObject.SetActive(true);

            Mapify.LogDebug("Creating Turntables");
            foreach (Turntable turntable in Object.FindObjectsOfType<Turntable>())
                CreateTurntable(turntable);

            Mapify.LogDebug("Creating Buffer Stops");
            foreach (Editor.BufferStop bufferStop in Object.FindObjectsOfType<Editor.BufferStop>())
                SetupBufferStop(bufferStop);

            RailManager.AlignAllTrackEnds();
            RailManager.TestConnections();
        }

        private static void CreateJunction(Switch sw)
        {
            Transform swTransform = sw.transform;
            VanillaAsset vanillaAsset = sw.GetComponent<VanillaObject>().asset;
            GameObject prefabClone = AssetCopier.Instantiate(vanillaAsset, active: false);
            Transform prefabCloneTransform = prefabClone.transform;
            Transform inJunction = prefabCloneTransform.Find("in_junction");
            Vector3 offset = prefabCloneTransform.position - inJunction.position;
            foreach (Transform child in prefabCloneTransform)
                child.transform.position += offset;
            prefabCloneTransform.SetPositionAndRotation(swTransform.position, swTransform.rotation);
            Junction junction = inJunction.gameObject.GetComponent<Junction>();
            junction.selectedBranch = 1;
            foreach (Junction.Branch branch in junction.outBranches)
                branch.track.generateColliders = true;
            prefabClone.SetActive(true);
        }

        private static void ConnectTracks(IEnumerable<Track> tracks)
        {
            // Ignore the warnings from not being able to find track, that's just a side effect of how we do things.
            LogType type = Debug.unityLogger.filterLogType;
            Debug.unityLogger.filterLogType = LogType.Error;

            foreach (Track track in tracks)
            {
                RailTrack railTrack = track.GetComponent<RailTrack>();
                if (railTrack.isJunctionTrack)
                    continue;
                if (railTrack.ConnectInToClosestJunction() == null)
                    railTrack.ConnectInToClosestBranch();
                if (railTrack.ConnectOutToClosestJunction() == null)
                    railTrack.ConnectOutToClosestBranch();
            }

            Debug.unityLogger.filterLogType = type;
        }

        private static void CreateTurntable(Turntable turntable)
        {
            TurntableController controller = null;
            bool usingDefaultTrack = false;
            foreach (VanillaObject vanillaObject in turntable.GetComponentsInChildren<VanillaObject>())
            {
                VanillaAsset vanillaAsset = vanillaObject.asset;
                if (vanillaAsset == VanillaAsset.TurntableTrack)
                    usingDefaultTrack = true;
                switch (vanillaAsset)
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

            if (controller == null)
                return;

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

            controller.turntableRotateLayered = AssetCopier.Instantiate(VanillaAsset.TurntableRotateLayered).GetComponent<LayeredAudio>();
            controller.turntable = turntableTrack;
            controller.gameObject.SetActive(true);
        }

        private static void SetupBufferStop(Editor.BufferStop bufferStop)
        {
            foreach (VanillaObject vanillaObject in bufferStop.GetComponentsInChildren<VanillaObject>())
                if (vanillaObject.asset == VanillaAsset.BufferStop)
                    vanillaObject.Replace();

            GameObject go = bufferStop.gameObject;
            go.SetActive(false);
            Layer.Train_Big_Collider.ApplyRecursive(go);
            Layer.Default.Apply(bufferStop.playerCollider);
            BufferStop dvBufferStop = go.AddComponent<BufferStop>();
            dvBufferStop.triggerCollider = bufferStop.GetComponent<BoxCollider>();
            dvBufferStop.bufferCollider = bufferStop.playerCollider;
            dvBufferStop.modelGO = new GameObject();
            go.SetActive(true);
        }

        private static void CreateSigns()
        {
            WorldMover.Instance.NewChild("Signs").AddComponent<SignPlacer>();
        }

        private static void CreateTrackLODs()
        {
            RailwayLodGenerator lodGenerator = new GameObject("Railway LOD Generator").AddComponent<RailwayLodGenerator>();
            // RailTrackRegistry#AllTracks causes issues here for some reason
            BaseType basedType = Object.FindObjectsOfType<RailTrack>().FirstOrDefault(rt => rt.baseType != null)?.baseType;
            if (basedType == null)
            {
                Mapify.LogError($"Failed to find a {nameof(BaseType)} to use for railway LOD generation!");
                return;
            }

            lodGenerator.profile = basedType.baseShape;
            GameObject ballastLodMaterialObject = AssetCopier.Instantiate(VanillaAsset.BallastLodMaterial);
            lodGenerator.mat = ballastLodMaterialObject.GetComponent<Renderer>().sharedMaterial;
            Object.Destroy(ballastLodMaterialObject);
        }
    }
}
