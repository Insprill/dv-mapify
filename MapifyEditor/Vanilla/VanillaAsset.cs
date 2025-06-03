namespace Mapify.Editor
{
    public enum VanillaAsset
    {
        #region Misc

        PlayerHouse = 1,
        Water = 2,
        CashRegister = 3,

        #endregion

        # region Stations

        CareerManager = 10,
        JobValidator = 11,
        TrashCan = 12,
        Dumpster = 13,
        LostAndFoundShed = 14,
        WarehouseMachine = 15,
        StationOffice1 = 16,
        StationOffice2 = 17,
        StationOffice3 = 18,
        StationOffice4 = 19,
        StationOffice5 = 20,
        StationOffice6 = 21,
        StationOffice7 = 22,

        # endregion

        #region Trackage

        BufferStopModel = 30,
        SwitchLeft = 31,
        SwitchRight = 32,
        SwitchLeftOuterSign = 33,
        SwitchRightOuterSign = 34,
        BallastLodMaterial = 35,

        #endregion

        #region Service Stations

        PitStopStation = 40,
        RefillMachineDiesel = 41,
        RefillMachineSand = 42,
        RefillMachineOil = 43,
        RefillMachineWater = 44,
        RefillMachineCoal = 45,
        RefillMachineBodyDamage = 46,
        RefillMachineWheelDamage = 47,
        RefillMachineMechanicalPowertrain = 48,
        RefillMachineElectricalPowertrain = 49,
        ServiceStationMarkerOpen = 50,
        ServiceStationMarkerClosed = 51,
        PitStopStationCoal1 = 52,
        PitStopStationCoal2 = 53,
        PitStopStationWater1 = 54,
        PitStopStationWater2 = 55,

        #endregion

        #region Turntables

        TurntablePit = 70,
        TurntableBridge = 71,
        TurntableTrack = 72,
        TurntableControlPanel = 73,
        TurntableControlShed = 74,
        TurntableRotateLayered = 75,

        #endregion

        #region Stores

        // store objects
        StoreObject = 80,
        StoreMesh = 81,

        // items
        // Don't forget to update the ItemType enum to match this.
        StoreItemBoombox = 82,
        StoreItemCassetteAlbum01 = 83,
        StoreItemCassetteAlbum02 = 84,
        StoreItemCassetteAlbum03 = 85,
        StoreItemCassetteAlbum04 = 86,
        StoreItemCassetteAlbum05 = 87,
        StoreItemCassetteAlbum06 = 88,
        StoreItemCassetteAlbum07 = 89,
        StoreItemCassetteAlbum08 = 90,
        StoreItemCassetteAlbum09 = 91,
        StoreItemCassetteAlbum10 = 92,
        StoreItemCassetteAlbum11 = 93,
        StoreItemCassetteAlbum12 = 94,
        StoreItemCassetteAlbum13 = 95,
        StoreItemCassetteAlbum14 = 96,
        StoreItemCassetteAlbum15 = 97,
        StoreItemCassetteAlbum16 = 98,
        StoreItemCassettePlaylist01 = 99,
        StoreItemCassettePlaylist02 = 100,
        StoreItemCassettePlaylist03 = 101,
        StoreItemCassettePlaylist04 = 102,
        StoreItemCassettePlaylist05 = 103,
        StoreItemCassettePlaylist06 = 104,
        StoreItemCassettePlaylist07 = 105,
        StoreItemCassettePlaylist08 = 106,
        StoreItemCassettePlaylist09 = 107,
        StoreItemCassettePlaylist10 = 108,
        StoreItemExpertShovel = 109,
        StoreItemGoldenShovel = 110,
        StoreItemLighter = 111,
        // StoreItemManualShunterBooklet = 112,
        // StoreItemManualSteamBooklet = 113,
        StoreItemRemoteController = 114,
        StoreItemShovel = 115,
        StoreItemStopwatch = 116,
        StoreItemLantern = 117,
        StoreItemFlashlight = 118,
        StoreItemEOTLantern = 119,

        // keys
        // StoreItemKey = 120,
        // StoreItemKeyCaboose = 121,
        // StoreItemKeyDE6Slug = 122,
        // StoreItemKeyDM1U = 123,

        // build 99
        StoreItemHandheldGameConsole = 124,
        StoreItemOiler = 125,
        StoreItemHandDrill = 126,
        StoreItemSolderingGun = 127,
        StoreItemCrimpingTool = 128,
        StoreItemFillerGun = 129,
        StoreItemHammer = 130,
        StoreItemDuctTape = 131,
        StoreItemSolderingWireReel = 132,
        StoreItemDistanceTracker = 133,
        StoreItemAntiWheelslipComputer = 134,
        StoreItemAutomaticTrainStop = 135,
        StoreItemAmpLimiter = 136,
        StoreItemWirelessMUController = 137,
        StoreItemClinometer = 138,
        StoreItemInfraredThermometer = 139,
        StoreItemDigitalSpeedometer = 140,
        StoreItemDigitalClock = 141,
        StoreItemShelfSmall = 142,
        StoreItemSteamEngineChecklist = 143,
        StoreItemSwitchButton = 144,
        StoreItemSwitchRotary = 145,
        StoreItemSwitchAnalog = 146,
        StoreItemMount70Long = 147,
        StoreItemMount90Square = 148,
        StoreItemMount90SquareLong = 149,
        StoreItemMount90Wide = 150,
        StoreItemMountLong = 151,
        StoreItemMountSmall = 152,
        StoreItemMountSquare = 153,
        StoreItemMountSquareVeryLong = 154,
        StoreItemMountVeryLong = 155,
        StoreItemSwivelLight = 156,
        StoreItemHeadlight = 157,
        StoreItemBrakeCylinderLEDBar = 158,
        StoreItemHanger = 159,
        StoreItemStickyTape = 160,
        StoreItemSunVisor = 161,
        StoreItemFlagMarkerBlue = 162,
        StoreItemFlagMarkerCyan = 163,
        StoreItemFlagMarkerGreen = 164,
        StoreItemFlagMarkerOrange = 165,
        StoreItemFlagMarkerPurple = 166,
        StoreItemFlagMarkerRed = 167,
        StoreItemFlagMarkerWhite = 168,
        StoreItemFlagMarkerYellow = 169,
        StoreItemLightBarBlue = 170,
        StoreItemLightBarCyan = 171,
        StoreItemLightBarGreen = 172,
        StoreItemLightBarOrange = 173,
        StoreItemLightBarPurple = 174,
        StoreItemLightBarRed = 175,
        StoreItemLightBarWhite = 176,
        StoreItemLightBarYellow = 177,
        StoreItemPaintCan = 178,
        StoreItemPaintCanMuseum = 179,
        StoreItemPaintCanSand = 180,
        StoreItemCrate = 181,
        StoreItemCratePlastic = 182,
        StoreItemPaperBox = 183,
        StoreItemBeaconAmber = 184,
        StoreItemBeaconRed = 185,
        StoreItemBeaconBlue = 186,
        StoreItemMount90SquareBig = 187,
        StoreItemMountSquareBig = 188,
        StoreItemOverheatingProtection = 189,
        StoreItemSwitchSetter = 190,
        StoreItemSwitchAlternating = 191,

        // build 99.4
        StoreItemItemContainerBriefcase = 192,
        StoreItemItemContainerFolder = 193,
        StoreItemNameplate = 194,
        StoreItemBrakeChecklist = 195,
        StoreItemItemContainerToolbox = 196,
        StoreItemShovelMount = 197,
        StoreItemBatteryCharger = 198,
        StoreItemItemContainerCrate = 199,
        StoreItemLabelMaker = 200,
        StoreItemMountStandBig = 201,
        StoreItemTaillight = 202,
        StoreItemDefectDetector = 203,
        StoreItemModernHeadlightR = 204,
        StoreItemModernHeadlightL = 205,
        StoreItemModernTaillightR = 206,
        StoreItemModernTaillightL = 207,
        StoreItemRemoteSignalBooster = 208,
        StoreItemProximitySensor = 209,
        StoreItemProximityReader = 210,
        StoreItemUniversalControlStand = 211,
        StoreItemItemContainerFolderBlue = 212,
        StoreItemItemContainerFolderRed = 213,
        StoreItemItemContainerFolderYellow = 214,
        StoreItemItemContainerRegistrator = 215

        #endregion
    }
}
