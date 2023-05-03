using System.Reflection;
using DV.CabControls;
using HarmonyLib;
using Mapify.Editor;
using Mapify.Utils;
using UnityEngine;

namespace Mapify.Patches
{
    [HarmonyPatch(typeof(TurntableController), "GetPushingInput")]
    public class TurntableController_GetPushingInput_Patch
    {
        private static readonly FieldInfo TurntableController_Field_PUSH_HANDLE_HALF_EXTENTS = AccessTools.DeclaredField(typeof(TurntableController), "PUSH_HANDLE_HALF_EXTENTS");

        private static bool Prefix(Transform handle)
        {
            if (handle == null)
                return false;
            BoxCollider collider = handle.GetComponent<BoxCollider>();
            if (collider != null)
                TurntableController_Field_PUSH_HANDLE_HALF_EXTENTS.SetValue(null, collider.size.Add(0.05f));
            return true;
        }
    }

    [HarmonyPatch(typeof(TurntableController), "FixedUpdate")]
    public class TurntableController_FixedUpdate_Patch
    {
        private static readonly MethodInfo TurntableController_Method_UpdateSnappingRangeSound = AccessTools.DeclaredMethod(typeof(TurntableController), "UpdateSnappingRangeSound", new[] { typeof(float) });

        private static bool Prefix(
            TurntableController __instance,
            LeverBase ___leverControl,
            float ___pushingPositiveDirectionValue,
            float ___pushingNegativeDirectionValue,
            ref float ___rotationSoundIntensity,
            ref bool ___snappingAngleSet,
            ref float ___snappingAngle,
            ref bool ___playTrackConnectedSound,
            ref float ___snappingDirection)
        {
            Turntable turntable = __instance.GetComponentInParent<Turntable>();
            if (!(turntable is Traverser traverser))
                return true;
            TurntableRailTrack turntableTrack = __instance.turntable;
            float currentPos = traverser.CurrentPos();
            const float leverSpeed = .5f;
            const float handleSpeed = .25f;
            float leverInput = ___leverControl.Value;
            float positiveInput = ___pushingPositiveDirectionValue != 0.0 ? ___pushingPositiveDirectionValue : Mathf.InverseLerp(0.55f, 1f, leverInput);
            float amount = 0;
            if (positiveInput > 0.0)
            {
                ___rotationSoundIntensity = positiveInput;
                ___snappingAngleSet = false;
                TurntableController_Method_UpdateSnappingRangeSound.Invoke(__instance, new object[] { turntableTrack.ClosestSnappingAngle() });
                amount = positiveInput * leverSpeed * Time.fixedDeltaTime;
            }
            else
            {
                float negativeInput = ___pushingNegativeDirectionValue != 0.0 ? ___pushingNegativeDirectionValue : Mathf.InverseLerp(0.45f, 0.0f, leverInput);
                if (negativeInput > 0.0)
                {
                    ___rotationSoundIntensity = negativeInput;
                    ___snappingAngleSet = false;
                    TurntableController_Method_UpdateSnappingRangeSound.Invoke(__instance, new object[] { turntableTrack.ClosestSnappingAngle() });
                    amount = -negativeInput * leverSpeed * Time.fixedDeltaTime;
                }
                else
                {
                    if (!___snappingAngleSet)
                    {
                        ___snappingAngle = turntableTrack.ClosestSnappingAngle();
                        ___snappingAngleSet = true;
                        if (___snappingAngle >= 0.0)
                        {
                            float f1 = ___snappingAngle - currentPos;
                            float f2 = ___snappingAngle - -currentPos;
                            ___snappingDirection = Mathf.Abs(f2) <= Mathf.Abs(f1) ? Mathf.Sign(f2) : Mathf.Sign(f1);
                        }
                    }

                    if (___snappingAngle >= 0.0)
                    {
                        if (!TurntableRailTrack.AnglesEqual(currentPos, ___snappingAngle) && !TurntableRailTrack.AnglesEqual(-currentPos, ___snappingAngle))
                        {
                            float f3 = ___snappingAngle - traverser.targetPosition;
                            float f4 = -f3;
                            amount = ___snappingDirection * Mathf.Min(Mathf.Abs(Mathf.Abs(f3) < Mathf.Abs(f4) ? f3 : f4), handleSpeed * Time.fixedDeltaTime);
                        }
                        else
                        {
                            // The Snapped event is normally invoked here, but it's only used in the tutorial.
                            ___playTrackConnectedSound = true;
                            ___snappingAngle = -1f;
                        }
                    }

                    ___rotationSoundIntensity = 0.0f;
                }
            }

            if (amount != 0)
            {
                traverser.targetPosition = Mathf.Clamp01(traverser.targetPosition + -amount);
                turntableTrack.RotateToTargetRotation();
            }


            return false;
        }
    }
}
