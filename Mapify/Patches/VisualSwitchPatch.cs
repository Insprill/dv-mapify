using System.Collections;
using HarmonyLib;
using Mapify.Utils;
using UnityEngine;

namespace Mapify.Patches
{
    /// <summary>
    /// Makes the visual part of the switch work with switches that have more than 2 branches.
    /// </summary>
    [HarmonyPatch(typeof(VisualSwitch), nameof(VisualSwitch.PlayAnimation))]
    public class VisualSwitch_PlayAnimation_Patch
    {
        private static readonly int Speed = Animator.StringToHash("speed");
        private const int ANIMATION_LAYER = 0;
        private const string STATE_NAME = "junction";

        private static bool Prefix(VisualSwitch __instance)
        {
            if (__instance.junction.outBranches.Count <= 2) return true;

            if (!__instance.animator) return false;

            __instance.EnableAnimator();

            var previousTime = __instance.animator.GetCurrentAnimatorStateInfo(ANIMATION_LAYER).normalizedTime;
            var newTime = MathUtils.Map(
                __instance.junction.selectedBranch,
                0, __instance.junction.outBranches.Count - 1,
                0, 1
            );

            var playForward = newTime > previousTime;

            // set animation direction
            var speed = playForward ? __instance.speedMult : -__instance.speedMult;
            __instance.animator.SetFloat(Speed, speed);

            // play animation
            __instance.animator.Play(STATE_NAME, ANIMATION_LAYER, previousTime);

            __instance.StopAllCoroutines();
            var routine = Pause(newTime, playForward, __instance.animator);
            __instance.StartCoroutine(routine);

            return false;
        }

        private static IEnumerator Pause(float pauseTime, bool playForward, Animator animator)
        {
            if (playForward)
            {
                while (animator.GetCurrentAnimatorStateInfo(ANIMATION_LAYER).normalizedTime < pauseTime)
                {
                    yield return null;
                }
            }
            else
            {
                while (animator.GetCurrentAnimatorStateInfo(ANIMATION_LAYER).normalizedTime > pauseTime)
                {
                    yield return null;
                }
            }

            // pause
            animator.SetFloat(Speed, 0);

            Mapify.Log($"{nameof(Pause)} {animator.GetCurrentAnimatorStateInfo(ANIMATION_LAYER).normalizedTime} PAUSED");
        }
    }
}
