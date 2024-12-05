using System.Collections.Generic;
using System.Linq;
using Mapify.Editor;
using Mapify.Utils;
using UnityEngine;

namespace Mapify.Components
{
    public class YardController_r: MonoBehaviour
    {
        // private enum SortStrategy
        // {
        //     Label,
        //     CarType
        // }

        private RailTrack detectorTrack;
        private Junction rootJunction;

        private string stationID;
        private string yardID;

        private Dictionary<byte, string> trackNumberToCarID = new();

        private bool hasBeenSetup = false;

        /// <summary>
        /// </summary>
        /// <param name="rootJunction_">The first junction in the tree of junctions (switches)</param>
        /// <param name="yardControllerValues"></param>
        public void Setup(Junction rootJunction_, YardController yardControllerValues)
        {
            detectorTrack = yardControllerValues.DetectorTrack.GetComponent<RailTrack>();
            rootJunction = rootJunction_;

            stationID = yardControllerValues.StationID;
            yardID = yardControllerValues.YardID;

            hasBeenSetup = true;
        }

        private void Start()
        {
            if (!hasBeenSetup)
            {
                Mapify.LogError($"{nameof(YardController_r)} on {gameObject.name} has not been setup");
                Destroy(this);
            }

            foreach (var trackNumber in RailTrackRegistry.Instance.GetTrackNumbersOfSubYard(stationID, yardID))
            {
                trackNumberToCarID.Add((byte)trackNumber, "");
            }

            if (trackNumberToCarID.Any()) return;

            Mapify.LogError($"{nameof(YardController_r)}: could not find track numbers for yard {stationID}-{yardID}");
            Destroy(this);
        }

        private void Update()
        {
            var detectedCar = detectorTrack.onTrackBogies.Select(bogie => bogie._car).FirstOrDefault();
            if(!detectedCar) return;

            var carTypeID = detectedCar.carLivery.parentType.id;

            foreach (var pair in trackNumberToCarID)
            {
                if(pair.Value != carTypeID) continue;
                Mapify.LogDebug($"{carTypeID} -> {stationID}-{yardID}-{pair.Key}");
                SetSwitches(pair.Key);
                return;
            }

            foreach (var pair in trackNumberToCarID)
            {
                if(pair.Value != "") continue;
                Mapify.LogDebug($"{carTypeID} -> {stationID}-{yardID}-{pair.Key}");
                trackNumberToCarID[pair.Key] = carTypeID;
                SetSwitches(pair.Key);
                return;
            }

            Mapify.LogError($"All {trackNumberToCarID.Values.Count} tracks taken");
            foreach (var pair in trackNumberToCarID)
            {
                Mapify.LogDebug($"{pair.Key} -> {pair.Value}");
            }
        }

        // set the switches, so that they form a path to trackNumber
        private void SetSwitches(byte trackNumber)
        {
            var goal = RailTrackRegistry.Instance.GetRailTrack(stationID, yardID, trackNumber);

            if (goal == null)
            {
                Mapify.LogError($"{nameof(SetSwitches)}: could not find track {stationID}/{yardID}/{trackNumber}");
                return;
            }

            //TODO cache the paths in Start?
            var start = rootJunction.inBranch.track;
            var path = PathFinder.FindPath(start, goal);

            if (!path.Any())
            {
                Mapify.LogError($"{nameof(SetSwitches)}: could not find path from {start.name} to {goal.name}");
                return;
            }

            Mapify.LogDebug($"{nameof(SetSwitches)}: path:");
            for (var index = 1; index < path.Count; index++)
            {
                var track = path[index];
                Mapify.LogDebug($"{track.name}");

                //if the track has an inJunction, it is a switch
                if (track.inJunction == null) continue;

                var previousTrack = path[index - 1];

                var outBranches = track.inJunction.outBranches;
                var outBranchNumber = GetBranchForTrack(outBranches, previousTrack);

                if(track.inJunction.selectedBranch == outBranchNumber){ continue; }

                track.inJunction.SwitchTo(outBranchNumber, Junction.SwitchMode.REGULAR);
            }
        }

        /// Returns the index of the branch that connects to the track
        private byte GetBranchForTrack(List<Junction.Branch> outBranches, RailTrack track)
        {
            for (byte branchIndex = 0; branchIndex < outBranches.Count; branchIndex++)
            {
                if (outBranches[branchIndex].track.outBranch.track == track)
                {
                    return branchIndex;
                }
            }

            Mapify.LogError($"{nameof(GetBranchForTrack)}: track {track} was not found in outBranches");

            Mapify.LogDebug("outbranch tracks:");
            foreach (var branch in outBranches)
            {
                Mapify.LogDebug(branch.track.outBranch.track.name);
            }

            return 0;
        }
    }
}
