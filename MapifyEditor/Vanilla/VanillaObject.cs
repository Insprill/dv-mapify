using UnityEngine;

namespace Mapify.Editor
{
    public class VanillaObject : MonoBehaviour
    {
        public VanillaAsset asset;
        public bool keepChildren = true;
        public Vector3 rotationOffset = Vector3.zero;

        /// <summary>
        /// Returns true if the VanillaAsset needs to be in the GameContent scene to work
        /// </summary>
        public bool BelongsInGameContent()
        {
            return asset == VanillaAsset.CareerManager ||
                   asset == VanillaAsset.JobValidator ||
                   asset == VanillaAsset.TrashCan ||
                   asset == VanillaAsset.Dumpster ||
                   asset == VanillaAsset.LostAndFoundShed ||
                   asset == VanillaAsset.WarehouseMachine ||
                   asset == VanillaAsset.PlayerHouse ||
                   asset == VanillaAsset.PitStopStationCoal1 ||
                   asset == VanillaAsset.PitStopStationCoal2 ||
                   asset == VanillaAsset.PitStopStationWater1 ||
                   asset == VanillaAsset.PitStopStationWater2 ||
                   asset == VanillaAsset.StationOffice1 ||
                   asset == VanillaAsset.StationOffice2 ||
                   asset == VanillaAsset.StationOffice3 ||
                   asset == VanillaAsset.StationOffice4 ||
                   asset == VanillaAsset.StationOffice5 ||
                   asset == VanillaAsset.StationOffice6 ||
                   asset == VanillaAsset.StationOffice7;
        }
    }
}
