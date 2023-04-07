using System.Collections.Generic;
using System.Linq;
using DV.Signs;
using Mapify.Editor;
using Mapify.Utils;
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
            ConnectTracks(tracks);

            foreach (Track track in tracks) track.gameObject.SetActive(true);

            foreach (VanillaObject vanillaObject in Object.FindObjectsOfType<VanillaObject>().Where(vo => vo.asset == VanillaAsset.BufferStop))
                vanillaObject.gameObject.Replace(AssetCopier.Instantiate(VanillaAsset.BufferStop));

            foreach (Switch sw in switches) GameObject.DestroyImmediate(sw.gameObject);

            RailManager.AlignAllTrackEnds();
            RailManager.TestConnections();
        }

        private static void CreateSigns()
        {
            WorldMover.Instance.NewChild("Signs").AddComponent<SignPlacer>();
        }

        private static void CreateJunction(Switch sw)
        {
            Transform swTransform = sw.transform;
            VanillaAsset vanillaAsset = sw.GetComponent<VanillaObject>().asset;
            bool isDivergingLeft = $"{vanillaAsset}".Contains("Left");
            GameObject prefabClone = AssetCopier.Instantiate(vanillaAsset, false);
            Transform prefabCloneTransform = prefabClone.transform;
            Transform inJunction = prefabCloneTransform.Find("in_junction");
            Vector3 offset = prefabCloneTransform.position - inJunction.position;
            foreach (Transform child in prefabCloneTransform)
                child.transform.position += offset;
            prefabCloneTransform.SetPositionAndRotation(swTransform.position, swTransform.rotation);
            GameObject throughTrack = sw.ThroughTrack.gameObject;
            GameObject divergingTrack = sw.DivergingTrack.gameObject;
            throughTrack.transform.SetParent(prefabCloneTransform, false);
            divergingTrack.transform.SetParent(prefabCloneTransform, false);
            Junction junction = inJunction.gameObject.AddComponent<Junction>();
            junction.selectedBranch = 1;
            prefabClone.GetComponentInChildren<VisualSwitch>().junction = junction;
            RailTrack throughRailTrack = throughTrack.GetComponent<RailTrack>();
            throughRailTrack.generateMeshes = false;
            throughRailTrack.inJunction = junction;
            throughRailTrack.overrideDefaultJointsSpan = true;
            throughRailTrack.jointsSpan = 5.1f;
            RailTrack divergingRailTrack = divergingTrack.GetComponent<RailTrack>();
            divergingRailTrack.generateMeshes = false;
            divergingRailTrack.inJunction = junction;
            divergingRailTrack.overrideDefaultJointsSpan = true;
            divergingRailTrack.jointsSpan = 5.1f;
            junction.outBranches = new List<Junction.Branch>(2) {
                new Junction.Branch(isDivergingLeft ? divergingRailTrack : throughRailTrack, true),
                new Junction.Branch(isDivergingLeft ? throughRailTrack : divergingRailTrack, true)
            };
            prefabClone.SetActive(true);
            throughRailTrack.gameObject.SetActive(true);
            divergingRailTrack.gameObject.SetActive(true);
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
    }
}
