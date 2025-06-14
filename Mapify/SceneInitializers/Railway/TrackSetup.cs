using System.Collections.Generic;
using System.Linq;
using DV;
using Mapify.Editor;
using Mapify.Utils;
using UnityEngine;

namespace Mapify.SceneInitializers.Railway
{
    [SceneSetupPriority(int.MinValue)]
    public class TrackSetup : SceneSetup
    {
        public override void Run()
        {
            Track[] tracks = Object.FindObjectsOfType<Track>().Where(t => !t.IsSwitch).ToArray();

            Mapify.LogDebug(() => "Creating RailTracks");
            CreateRailTracks(tracks);

            Mapify.LogDebug(() => "Creating Junctions");
            CreateJunctions();

            tracks.SetActive(true);

            Mapify.LogDebug(() => "Connecting tracks");
            ConnectTracks(tracks);

            AlignAllTrackEnds();
            TestConnections();
        }

        // copied from B99.3 RailManager class
        private void AlignAllTrackEnds()
        {
            foreach (RailTrack railTrack in Object.FindObjectsOfType<RailTrack>())
            {
                if (!railTrack.dontChange)
                    railTrack.TryAlignHandles();
            }
            Debug.Log((object) "Aligned all track ends");
        }

        // copied from B99.3 RailManager class
        private void TestConnections()
        {
            var objectsOfType = Object.FindObjectsOfType<RailTrack>();
            var flag = false;
            foreach (var track1 in objectsOfType)
            {
                if ((bool) (Object) track1.inJunction && !track1.inJunction.HasBranch(new Junction.Branch(track1, true)))
                {
                    Debug.LogError($"Junction '{track1.inJunction.name}' doesn't have track '{track1.name}' assigned", track1.inJunction);
                    flag = true;
                }
                if ((bool) (Object) track1.outJunction && !track1.outJunction.HasBranch(new Junction.Branch(track1, false)))
                {
                    Debug.LogError($"Junction '{track1.outJunction.name}' doesn't have track '{track1.name}' assigned", track1.outJunction);
                    flag = true;
                }
                if (track1.inIsConnected && !(bool) (Object) track1.inJunction)
                {
                    var track2 = track1.inBranch.track;
                    var branch = track1.inBranch.first ? track2.inBranch : track2.outBranch;
                    if (!(branch.track == track1) || !branch.first)
                    {
                        Debug.LogError($"Track '{track2.name}'s IN is not connected to track {track1.name}. Manually set or reconnect branches", track2);
                        flag = true;
                    }
                }
                if (track1.outIsConnected && !(bool) (Object) track1.outJunction)
                {
                    var track3 = track1.outBranch.track;
                    var branch = track1.outBranch.first ? track3.inBranch : track3.outBranch;
                    if (!(branch.track == track1) || branch.first)
                    {
                        Debug.LogError($"Track '{track3.name}'s OUT is not connected to track {track1.name}. Manually set or reconnect branches", track3);
                        flag = true;
                    }
                }
            }
            if (!flag)
                Debug.Log("Checked all connections, no errors were found");
            else
                Debug.LogError("Problems found when checking connections, see errors above");
        }

        private static void CreateRailTracks(IEnumerable<Track> tracks)
        {
            foreach (Track track in tracks)
            {
                track.gameObject.SetActive(false);
                if (!track.IsSwitch && !track.IsTurntable)
                    track.name = track.LogicName;
                RailTrack railTrack = track.gameObject.AddComponent<RailTrack>();
                railTrack.generateColliders = !track.IsTurntable;
                railTrack.dontChange = false;
                railTrack.age = (int)track.age;
                railTrack.ApplyRailType();
            }
        }

        private static void CreateJunctions()
        {
            foreach (Switch sw in Object.FindObjectsOfType<Switch>())
            {
                Transform swTransform = sw.transform;
                VanillaAsset vanillaAsset = sw.GetComponent<VanillaObject>().asset;
                GameObject prefabClone = AssetCopier.Instantiate(vanillaAsset, false);
                Transform prefabCloneTransform = prefabClone.transform;
                Transform inJunction = prefabCloneTransform.Find("in_junction");
                Vector3 offset = prefabCloneTransform.position - inJunction.position;
                foreach (Transform child in prefabCloneTransform)
                    child.transform.position += offset;
                prefabCloneTransform.SetPositionAndRotation(swTransform.position, swTransform.rotation);

                Junction junction = inJunction.GetComponent<Junction>();
                junction.selectedBranch = (byte) (sw.IsLeft
                    ? sw.defaultState == Switch.StandSide.THROUGH
                        ? 1
                        : 0
                    : sw.defaultState == Switch.StandSide.THROUGH
                        ? 0
                        : 1
                    );

                foreach (Junction.Branch branch in junction.outBranches)
                    branch.track.generateColliders = true;

                prefabClone.transform.SetParent(RailTrackRegistry.Instance.TrackRootParent);
                prefabClone.SetActive(true);
            }
        }

        private static void ConnectTracks(IEnumerable<Track> tracks)
        {
            foreach (Track track in tracks)
            {
                RailTrack railTrack = track.GetComponent<RailTrack>();
                if (railTrack.isJunctionTrack)
                    continue;

                railTrack.ConnectInToClosestJunctionOrBranch();
                railTrack.ConnectOutToClosestJunctionOrBranch();
            }
        }
    }
}
