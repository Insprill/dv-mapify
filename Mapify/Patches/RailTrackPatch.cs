using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using Mapify.Editor;

namespace Mapify.Patches
{
    [HarmonyPatch(typeof(RailTrack), nameof(RailTrack.ConnectToClosestBranch))]
    public static class RailTrack_ConnectToClosestBranch_Patch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return RailTrack_ConnectX.Transpile(instructions);
        }
    }

    [HarmonyPatch(typeof(RailTrack), nameof(RailTrack.ConnectInToClosestJunction))]
    public static class RailTrack_ConnectInToClosestJunction_Patch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return RailTrack_ConnectX.Transpile(instructions);
        }
    }

    [HarmonyPatch(typeof(RailTrack), nameof(RailTrack.ConnectOutToClosestJunction))]
    public static class RailTrack_ConnectOutToClosestJunction_Patch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return RailTrack_ConnectX.Transpile(instructions);
        }
    }

    public static class RailTrack_ConnectX
    {
        internal static IEnumerable<CodeInstruction> Transpile(IEnumerable<CodeInstruction> instructions)
        {
            CodeInstruction[] codes = instructions.ToArray();
            foreach (CodeInstruction code in codes)
                // Changes the range of the RailTrack#ConnectX methods to our snap range
                if (code.opcode == OpCodes.Ldc_R4 && (float)code.operand - 5f < 0.001)
                    code.operand = Track.SNAP_RANGE;

            return codes;
        }
    }

    /// <summary>
    ///     Fixes a NullReferenceException when the provided branch is null.
    ///     This can happen when RailTrack#ConnectToClosestBranch fails to find a branch.
    /// </summary>
    [HarmonyPatch(typeof(RailTrack), nameof(RailTrack.MovePointToBranchEnd))]
    public static class RailTrack_MovePointToBranchEnd_Patch
    {
        private static bool Prefix(Junction.Branch branch)
        {
            return branch != null;
        }
    }
}
