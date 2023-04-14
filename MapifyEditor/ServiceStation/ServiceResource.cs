using System;

namespace Mapify.Editor
{
    public enum ServiceResource
    {
        Fuel,
        Sand,
        Oil,
        Water,
        Coal,
        CarDamage,
        WheelDamage,
        EngineDamage
    }

    public static class ServiceResourceExtensions
    {
        public static VanillaAsset ToVanillaAsset(this ServiceResource resource)
        {
            switch (resource)
            {
                case ServiceResource.Fuel:
                    return VanillaAsset.RefillMachineFuel;
                case ServiceResource.Sand:
                    return VanillaAsset.RefillMachineSand;
                case ServiceResource.Oil:
                    return VanillaAsset.RefillMachineOil;
                case ServiceResource.Water:
                    return VanillaAsset.RefillMachineWater;
                case ServiceResource.Coal:
                    return VanillaAsset.RefillMachineCoal;
                case ServiceResource.CarDamage:
                    return VanillaAsset.RefillMachineCarDamage;
                case ServiceResource.WheelDamage:
                    return VanillaAsset.RefillMachineWheelDamage;
                case ServiceResource.EngineDamage:
                    return VanillaAsset.RefillMachineEngineDamage;
                default:
                    throw new ArgumentOutOfRangeException(nameof(resource), resource, null);
            }
        }
    }
}
