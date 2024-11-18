using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;
using System.Runtime.InteropServices;
using Mapify.Editor;
using Mapify.Utils;
using UnityEngine;

namespace Mapify.Components
{
    public class SwitchController: MonoBehaviour
    {
        // public Track DetectorTrack;
        private Junction rootJunction;

        private bool hasBeenSetup = false;

        public void Setup(Junction junction)
        {
            // DetectorTrack = detectorTrack;
            rootJunction = junction;

            hasBeenSetup = true;
        }

        private void Start()
        {
            if (!hasBeenSetup)
            {
                Mapify.LogError($"{nameof(SwitchController)} on {gameObject.name} has not been setup yet");
                Destroy(this);
            }
        }

        private void Update()
        {
            int track = -1;

            if (Input.GetKeyDown(KeyCode.Keypad1))
            {
                track = 1;
            }
            if (Input.GetKeyDown(KeyCode.Keypad2))
            {
                track = 2;
            }
            if (Input.GetKeyDown(KeyCode.Keypad3))
            {
                track = 3;
            }
            if (Input.GetKeyDown(KeyCode.Keypad4))
            {
                track = 4;
            }

            if (track == -1) { return; }

            SetSwitches((byte)track);
        }

        // set the switches, so that they form a path to trackNumber
        private void SetSwitches(byte trackNumber)
        {
            //TODO
            var stationID = "station";
            var yardID = "B";

            var goal = RailTrackRegistry.Instance.GetRailTrack(stationID, yardID, trackNumber);

            if (goal == null)
            {
                Mapify.LogError($"{nameof(SetSwitches)}: could not find track {stationID}/{yardID}/{trackNumber}");
                return;
            }

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
        private int GetBranchForTrack(List<Junction.Branch> outBranches, RailTrack track)
        {
            for (int branchIndex = 0; branchIndex < outBranches.Count; branchIndex++)
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
