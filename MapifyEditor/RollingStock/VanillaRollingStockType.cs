namespace Mapify.Editor
{
    // Globals.G.Types._carTypesById.Select(car => car.Key)
    // Use Enum.GetName to turn it into a string ID
    public enum VanillaRollingStockType : byte
    {
        Autorack = 0,
        Boxcar = 1,
        BoxcarMilitary = 2,
        Caboose = 3,
        Flatbed = 4,
        FlatbedMilitary = 5,
        FlatbedStakes = 6,
        FlatbedShort = 7,
        Gondola = 8,
        HandCar = 9,
        Hopper = 10,
        HopperCovered = 11,
        LocoDE2 = 12,
        LocoDE6 = 13,
        LocoDE6Slug = 14,
        LocoDH4 = 15,
        LocoDM3 = 16,
        LocoS282A = 17,
        LocoS282B = 18,
        LocoS060 = 19,
        NuclearFlask = 20,
        Passenger = 21,
        Stock = 22,
        Refrigerator = 23,
        TankChem = 24,
        TankGas = 25,
        TankOil = 26,
        TankShortFood = 27,
        LocoMicroshunter = 28,
        LocoDM1U = 29
    }

    public enum VanillaLocomotiveType : byte
    {
        LocoDE2 = 0,
        LocoDE6 = 1,
        LocoDH4 = 10,
        LocoDM3 = 20,
        LocoS282A = 30,
        LocoS282B = 31,
        LocoS060 = 40,
    }
}
