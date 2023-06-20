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
                switch (vanillaObject.asset)
                {
                    case VanillaAsset.CareerManager:
                    case VanillaAsset.JobValidator:
                    case VanillaAsset.TrashCan:
                    case VanillaAsset.Dumpster:
                    case VanillaAsset.LostAndFoundShed:
                    case VanillaAsset.WarehouseMachine:
                    case VanillaAsset.PlayerHouse:
                        vanillaObject.Replace();
                        break;
                    case VanillaAsset.StationOffice1:
                    case VanillaAsset.StationOffice2:
                    case VanillaAsset.StationOffice3:
                    case VanillaAsset.StationOffice4:
                    case VanillaAsset.StationOffice5:
                    case VanillaAsset.StationOffice6:
                    case VanillaAsset.StationOffice7:
                        GameObject go = vanillaObject.Replace();
                        // todo: make this show in the correct location instead of removing it
                        Transform youAreHereFlag = go.transform.FindChildByName("PinRed");
                        if (youAreHereFlag != null)
                            Object.Destroy(youAreHereFlag.gameObject);
                        break;
                }
        }
    }
}
