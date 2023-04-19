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
        private const float TURNTABLE_HEIGHT_OFFSET = 2.5f;

        private static readonly FieldInfo TurntableRailTrack_Field__track = AccessTools.DeclaredField(typeof(TurntableRailTrack), "_track");

        public static void SceneLoaded(Scene scene)
        {
            Transform railwayParent = WorldMover.Instance.NewChild("[railway]").transform;
            foreach (Transform transform in scene.GetRootGameObjects().Select(go => go.transform))
                transform.SetParent(railwayParent);
            SetupRailTracks();
            CreateSigns();
        }

        private static void SetupRailTracks()
        {
            Main.Logger.Log("Creating RailTracks");

            Track[] tracks = Object.FindObjectsOfType<Track>().Where(t => !t.IsSwitch && !t.IsTurntable).ToArray();
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
                railTrack.dontChange = false;
                railTrack.age = age;
                railTrack.ApplyRailType();
            }

            Main.Logger.Log("Creating Junctions");
            foreach (Switch sw in Object.FindObjectsOfType<Switch>())
                CreateJunction(sw);

            Main.Logger.Log("Connecting tracks");
            ConnectTracks(tracks);

            foreach (Track track in tracks)
                track.gameObject.SetActive(true);

            VanillaObject[] vanillaObjects = Object.FindObjectsOfType<VanillaObject>();

            Main.Logger.Log("Creating Turntables");
            SetupTurntables(vanillaObjects);

            Main.Logger.Log("Creating Buffer Stops");
            SetupBufferStops(vanillaObjects);

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

        private static void SetupTurntables(VanillaObject[] vanillaObjects)
        {
            foreach (VanillaObject vanillaObject in vanillaObjects.Where(vo => vo.asset == VanillaAsset.TurntableTrack || vo.asset == VanillaAsset.TurntablePit))
            {
                Transform t = vanillaObject.gameObject.Replace(AssetCopier.Instantiate(vanillaObject.asset)).transform;
                Vector3 pos = t.position;
                pos.y += TURNTABLE_HEIGHT_OFFSET;
                t.position = pos;
            }

            TurntableRailTrack[] turntableTracks = Object.FindObjectsOfType<TurntableRailTrack>();
            foreach (TurntableRailTrack track in turntableTracks)
            {
                if (track.Track != null)
                    Object.Destroy(track.Track);
                RailTrack newTrack = track.gameObject.AddComponent<RailTrack>();
                newTrack.generateMeshes = false;
                newTrack.generateColliders = false;
                TurntableRailTrack_Field__track.SetValue(track, newTrack);
                track.uniqueID = $"{track.transform.position.GetHashCode()}";
                track.trackEnds = track.FindTrackEnds();
            }

            foreach (VanillaObject vanillaObject in vanillaObjects.Where(vo => vo.asset == VanillaAsset.TurntableControlPanel))
            {
                Transform controlPanel = vanillaObject.gameObject.Replace(AssetCopier.Instantiate(vanillaObject.asset, active: false)).transform;
                Vector3 controlPanelPos = controlPanel.position.AddY(-TURNTABLE_HEIGHT_OFFSET); // The control panel is a child of the pit so they rotate together, but our Y pos isn't offset.
                controlPanel.position = controlPanelPos;
                TurntableController controller = controlPanel.GetComponent<TurntableController>();
                controller.turntableRotateLayered = AssetCopier.Instantiate(VanillaAsset.TurntableRotateLayered).GetComponent<LayeredAudio>();
                controller.turntable = turntableTracks.OrderBy(t => (t.transform.position - controlPanelPos).sqrMagnitude).FirstOrDefault();
                controlPanel.gameObject.SetActive(true);
            }
        }

        private static void SetupBufferStops(IEnumerable<VanillaObject> vanillaObjects)
        {
            foreach (VanillaObject vanillaObject in vanillaObjects.Where(vo => vo.asset == VanillaAsset.BufferStop))
                vanillaObject.gameObject.Replace(AssetCopier.Instantiate(vanillaObject.asset));
        }

        private static void CreateSigns()
        {
            WorldMover.Instance.NewChild("Signs").AddComponent<SignPlacer>();
        }
    }
}
