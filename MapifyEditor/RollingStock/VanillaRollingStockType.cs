using System;

namespace Mapify.Editor
{
    public enum VanillaRollingStockType : byte
    {
        DE2 = 0,
        DE6 = 1,
        DH4 = 10,
        DM3 = 20,
        S282A = 30,
        S282B = 31,
        S060 = 40
    }

    public static class VanillaRollingStockTypeExtensions
    {
        public static string ToV2(this VanillaRollingStockType vanillaRollingStockType)
        {
            return vanillaRollingStockType switch {
                VanillaRollingStockType.DE2 => "LocoDE2",
                VanillaRollingStockType.DE6 => "LocoDE6",
                VanillaRollingStockType.DH4 => "LocoDH4",
                VanillaRollingStockType.DM3 => "LocoDM3",
                VanillaRollingStockType.S282A => "LocoS282A",
                VanillaRollingStockType.S282B => "LocoS282B",
                VanillaRollingStockType.S060 => "LocoS060",
                _ => throw new ArgumentOutOfRangeException(nameof(vanillaRollingStockType), vanillaRollingStockType, null)
            };
        }
    }
}
