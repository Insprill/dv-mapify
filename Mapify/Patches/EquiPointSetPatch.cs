using System;
using DV.PointSet;
using HarmonyLib;

namespace Mapify.Patches
{
    [HarmonyPatch(typeof(EquiPointSet), nameof(EquiPointSet.ResampleEquidistant))]
    public static class EquiPointSet_ResampleEquidistant_Patch
    {
        private static bool Prefix(EquiPointSet source, float pointSpacing, ref EquiPointSet __result)
        {
            if (pointSpacing <= source.span / 2.0)
                return true;
            __result = new EquiPointSet {
                points = Array.Empty<EquiPointSet.Point>()
            };
            return false;
        }
    }
}
