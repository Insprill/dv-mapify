using System.Collections.Generic;
using Mapify.Editor;
using Mapify.Editor.Utils;
using Mapify.Utils;
using UnityEngine;

namespace Mapify.SceneInitializers.GameContent
{
    [SceneSetupPriority(-10)]
    public class VanillaAssetSetup : SceneSetup
    {
        public override void Run()
        {
            foreach (VanillaObject vanillaObject in Object.FindObjectsOfType<VanillaObject>())
            {
                //only these belong in the gamecontent scene
                switch (vanillaObject.asset)
                {
                    case VanillaAsset.CareerManager:
                    case VanillaAsset.JobValidator:
                    case VanillaAsset.TrashCan:
                    case VanillaAsset.Dumpster:
                    case VanillaAsset.LostAndFoundShed:
                    case VanillaAsset.WarehouseMachine:
                    case VanillaAsset.PlayerHouse:
                    case VanillaAsset.PitStopStationCoal1:
                    case VanillaAsset.PitStopStationCoal2:
                    case VanillaAsset.PitStopStationWater1:
                    case VanillaAsset.PitStopStationWater2:
                    case VanillaAsset.StationOffice1:
                    case VanillaAsset.StationOffice2:
                    case VanillaAsset.StationOffice3:
                    case VanillaAsset.StationOffice4:
                    case VanillaAsset.StationOffice5:
                    case VanillaAsset.StationOffice6:
                    case VanillaAsset.StationOffice7:
                        vanillaObject.Replace();
                        break;
                }
            }
        }
    }
}
