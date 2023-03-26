using System.Linq;
using DV.Signs;
using Mapify.Editor;
using UnityEngine;

namespace Mapify.SceneInitializers
{
    public static class RailwaySceneInitializer
    {
        public static void SceneLoaded()
        {
            SetupRailTracks();
            CreateSigns();
        }

        private static void SetupRailTracks()
        {
            Main.Logger.Log("Creating RailTracks");
            Track[] tracks = Object.FindObjectsOfType<Track>();
            foreach (Track track in tracks)
            {
                track.gameObject.SetActive(false);
                RailTrack railTrack = track.gameObject.AddComponent<RailTrack>();
                railTrack.dontChange = false;
                railTrack.age = track.age;
                railTrack.ApplyRailType();
            }

            Main.Logger.Log("Creating Junctions");
            Switch[] switches = Object.FindObjectsOfType<Switch>();
            foreach (Switch sw in switches) CreateJunction(sw);

            Main.Logger.Log("Connecting tracks");
            foreach (Track track in tracks)
            {
                ConnectRailTrack(track);
                track.gameObject.SetActive(true);
            }

            foreach (Switch sw in switches) GameObject.DestroyImmediate(sw.gameObject);

            RailManager.AlignAllTrackEnds();
            RailManager.TestConnections();
        }

        private static void CreateSigns()
        {
            new GameObject("Signs").AddComponent<SignPlacer>();
        }

        private static void CreateJunction(Switch sw)
        {
            Transform swTransform = sw.transform;
            GameObject prefabClone = GameObject.Instantiate(VanillaRailwaySceneInitializer.GetSwitchPrefab(sw.SwitchPrefabName));
            Transform prefabCloneTransform = prefabClone.transform;
            Transform inJunction = prefabCloneTransform.Find("in_junction");
            Vector3 offset = prefabCloneTransform.position - inJunction.position;
            foreach (Transform child in prefabCloneTransform)
                child.transform.position += offset;
            prefabCloneTransform.SetPositionAndRotation(swTransform.position, swTransform.rotation);
            GameObject throughTrack = sw.throughTrack.gameObject;
            GameObject divergingTrack = sw.divergingTrack.gameObject;
            throughTrack.transform.SetParent(prefabCloneTransform, false);
            divergingTrack.transform.SetParent(prefabCloneTransform, false);
            sw.tracksParent = prefabClone;
            Junction junction = inJunction.gameObject.AddComponent<Junction>();
            junction.selectedBranch = 1;
            prefabClone.GetComponentInChildren<VisualSwitch>().junction = junction;
            junction.inBranch = new Junction.Branch(sw.inTrack.GetComponent<RailTrack>(), sw.inTrackFirst);
            RailTrack throughRailTrack = throughTrack.GetComponent<RailTrack>();
            throughRailTrack.generateMeshes = false;
            RailTrack divergingRailTrack = divergingTrack.GetComponent<RailTrack>();
            divergingRailTrack.generateMeshes = false;
            junction.outBranches = new[] {
                new Junction.Branch(throughRailTrack, true),
                new Junction.Branch(divergingRailTrack, true)
            }.ToList();
            prefabClone.SetActive(true);
        }

        private static void ConnectRailTrack(Track track)
        {
            RailTrack railTrack = track.gameObject.GetComponent<RailTrack>();
            if (track.inTrack != null)
            {
                RailTrack inRailTrack = track.inTrack.GetComponent<RailTrack>();
                railTrack.inBranch = new Junction.Branch(inRailTrack, track.inTrackFirst);
            }

            if (track.outTrack != null)
            {
                RailTrack outRailTrack = track.outTrack.GetComponent<RailTrack>();
                railTrack.outBranch = new Junction.Branch(outRailTrack, track.outTrackFirst);
            }

            if (track.inSwitch)
                railTrack.inJunction = track.inSwitch.tracksParent.GetComponentInChildren<Junction>(true);

            if (track.outSwitch)
                railTrack.outJunction = track.outSwitch.tracksParent.GetComponentInChildren<Junction>(true);
        }
    }
}
