using System;
using DV;
using DV.Utils;
using UnityEngine;

namespace Mapify.Components
{
    public class TransferTableController: TurntableController
    {
        public const float DEFAULT_TURNTABLE_RADIUS = 12.324f;
        public const float DEFAULT_TURNTABLE_CIRCUMFERENCE = DEFAULT_TURNTABLE_RADIUS * 2 * Mathf.PI;
        public const float ONE_DEGREE_DISTANCE = DEFAULT_TURNTABLE_CIRCUMFERENCE / 360f;

        private const float MAX_MOVE_SPEED = ONE_DEGREE_DISTANCE * MAX_ROTATION_SPEED_DEGREES_PER_SEC;

        public event Action Snapped2;
        private bool snappingAngleHasValue = false;

        public void CopyValues(TurntableController _base)
        {
            leverGO = _base.leverGO;
            turntable = _base.turntable;
            speedMultiplier = _base.speedMultiplier;
            snapRangeEnterSound = _base.snapRangeEnterSound;
            trackConnectedSound = _base.trackConnectedSound;
            turntableRotateLayered = _base.turntableRotateLayered;
            rotationSoundIntensity = _base.rotationSoundIntensity;
            leverControl = _base.leverControl;
            snappingAngleSet = _base.snappingAngleSet;
            snappingAngle = _base.snappingAngle;
            // snappingDirection = _base.snappingDirection; //we dont use this
            playTrackConnectedSound = _base.playTrackConnectedSound;
            playSnapRangeEnterSound = _base.playSnapRangeEnterSound;
            lastSnappingAnglePlayed = _base.lastSnappingAnglePlayed;
            playerLayerMask = _base.playerLayerMask;
            playerOverlapResults = _base.playerOverlapResults;
            pushingPositiveDirectionValue = _base.pushingPositiveDirectionValue;
            pushingNegativeDirectionValue = _base.pushingNegativeDirectionValue;

            PlayerControlAllowed = _base.PlayerControlAllowed;
        }

        private new void FixedUpdate()
        {
            var transferTableTrack = (TransferTableRailTrack)turntable;

            // =======================

            if (!WorldStreamingInit.IsLoaded || SingletonBehaviour<PausePhysicsHandler>.Instance.PhysicsHandlingInProcess)
                return;

            var leverInput = PlayerControlAllowed ? leverControl.Value : PUSHING_DOT_PRODUCT_THRESHOLD;

            var positiveInput = pushingPositiveDirectionValue != 0.0 ? pushingPositiveDirectionValue : Mathf.InverseLerp(LEVER_POSITIVE_DIRECTION_THRESHOLD, 1f, leverInput);

            if (positiveInput > 0.0)
            {
                rotationSoundIntensity = positiveInput;
                snappingAngleSet = false;
                if (transferTableTrack.IsTrackEndWithinSnappingRange(out var snapPosition))
                {
                    UpdateSnappingRangeSound(snapPosition);
                }

                transferTableTrack.targetYRotation = transferTableTrack.PositionRange(transferTableTrack.targetYRotation + positiveInput * MAX_MOVE_SPEED * speedMultiplier * Time.fixedDeltaTime);
                transferTableTrack.MoveToTargetPosition();
            }
            else
            {
                var pushingValueNegative = pushingNegativeDirectionValue != 0.0 ? pushingNegativeDirectionValue : Mathf.InverseLerp(LEVER_NEGATIVE_DIRECTION_THRESHOLD, 0.0f, leverInput);
                if (pushingValueNegative > 0.0)
                {
                    rotationSoundIntensity = pushingValueNegative;
                    snappingAngleSet = false;
                    if (transferTableTrack.IsTrackEndWithinSnappingRange(out var snapPosition))
                    {
                        UpdateSnappingRangeSound(snapPosition);
                    }
                    transferTableTrack.targetYRotation = transferTableTrack.PositionRange(transferTableTrack.targetYRotation + (float) (-(double) pushingValueNegative * MAX_MOVE_SPEED) * speedMultiplier * Time.fixedDeltaTime);
                    transferTableTrack.MoveToTargetPosition();
                }
                else
                {
                    if (!snappingAngleSet)
                    {
                        snappingAngleHasValue = transferTableTrack.IsTrackEndWithinSnappingRange(out snappingAngle);
                        snappingAngleSet = true;
                    }
                    if (snappingAngleHasValue)
                    {
                        if (!TransferTableRailTrack.PositionsEqual(  transferTableTrack.currentYRotation, snappingAngle))
                        {
                            var moveDelta = snappingAngle - transferTableTrack.targetYRotation;
                            var maxMoveStep = MAX_SNAPPING_ROTATION_SPEED_DEGREES_PER_SEC * Time.fixedDeltaTime;
                            var moveStep = Mathf.Min(moveDelta, maxMoveStep);

                            transferTableTrack.targetYRotation = transferTableTrack.PositionRange(
                                transferTableTrack.targetYRotation + moveStep
                            );
                            transferTableTrack.MoveToTargetPosition();
                        }
                        else
                        {
                            var snapped = Snapped2;
                            if (snapped != null)
                                snapped();
                            playTrackConnectedSound = true;
                            snappingAngleHasValue = false;
                        }
                    }
                    rotationSoundIntensity = 0.0f;
                }
            }
        }

        public new void UpdateSnappingRangeSound(float currentSnappingPosition)
        {
            var playedSoundForThisPosition = TransferTableRailTrack.PositionsEqual(currentSnappingPosition, lastSnappingAnglePlayed);
            if (playedSoundForThisPosition)
                return;
            playSnapRangeEnterSound = true;
            lastSnappingAnglePlayed = currentSnappingPosition;
        }
    }
}
