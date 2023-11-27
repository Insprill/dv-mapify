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
                var yes = new List<VanillaAsset>
                {
                    VanillaAsset.CareerManager,
                    VanillaAsset.JobValidator,
                    VanillaAsset.TrashCan,
                    VanillaAsset.Dumpster,
                    VanillaAsset.LostAndFoundShed,
                    VanillaAsset.WarehouseMachine,
                    VanillaAsset.PlayerHouse,
                    VanillaAsset.PitStopStationCoal1,
                    VanillaAsset.PitStopStationCoal2,
                    VanillaAsset.PitStopStationWater1,
                    VanillaAsset.PitStopStationWater2,
                    VanillaAsset.StationOffice1,
                    VanillaAsset.StationOffice2,
                    VanillaAsset.StationOffice3,
                    VanillaAsset.StationOffice4,
                    VanillaAsset.StationOffice5,
                    VanillaAsset.StationOffice6,
                    VanillaAsset.StationOffice7
                };

                if (!yes.Contains(vanillaObject.asset))
                {
                    continue;
                }

                vanillaObject.Replace();
            }
        }
    }
}
