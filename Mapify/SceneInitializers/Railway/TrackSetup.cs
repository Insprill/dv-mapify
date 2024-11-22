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

            RailManager.AlignAllTrackEnds();
            RailManager.TestConnections();
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

                prefabClone.transform.SetParent(WorldData.Instance.TrackRootParent);
                prefabClone.SetActive(true);
            }
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
