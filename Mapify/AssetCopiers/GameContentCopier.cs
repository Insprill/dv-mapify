using System;
using System.Collections.Generic;
using System.Linq;
using DV.Shops;
using Mapify.Editor;
using Mapify.Editor.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Mapify.SceneInitializers.Vanilla.GameContent
{
    public class GameContentCopier : AssetCopier
    {
        public const int START_IDX = short.MaxValue;
        private static int nextIdx = START_IDX;

        protected override IEnumerator<(VanillaAsset, GameObject)> ToSave(GameObject gameObject)
        {
            string name = gameObject.name;

            if (name.StartsWith("[") && name.EndsWith("]") && name != WorldStreamingInit.ORIGIN_SHIFT_CONTENT)
            {
                VanillaAsset idx = (VanillaAsset)nextIdx++;
                Mapify.LogDebug(() => $"Saved Vanilla Asset {idx} ({name})");
                yield return (idx, gameObject);
            }

            if (name != WorldStreamingInit.ORIGIN_SHIFT_CONTENT)
                yield break;

            #region Misc

            yield return (VanillaAsset.Water, gameObject.FindChildByName("water"));
            yield return (VanillaAsset.PlayerHouse, gameObject.FindChildByName("PlayerHouse01"));

            #endregion

            #region Stations

            yield return (VanillaAsset.CareerManager, GameObject.Instantiate(gameObject.FindChildByName("CareerManager")));
            yield return (VanillaAsset.JobValidator, GameObject.Instantiate(gameObject.FindChildByName("JobValidator")));
            yield return (VanillaAsset.TrashCan, GameObject.Instantiate(gameObject.FindChildByName("JobTrashBin")));
            yield return (VanillaAsset.Dumpster, gameObject.FindChildByName("SkipDumpster"));
            yield return (VanillaAsset.LostAndFoundShed, gameObject.FindChildByName("OldShed"));
            yield return (VanillaAsset.WarehouseMachine, gameObject.FindChildByName("WarehouseMachineHMB"));

            for (int i = 1; i <= 7; i++)
            {
                string enumName = $"StationOffice{i}";
                if (Enum.TryParse(enumName, out VanillaAsset asset))
                    yield return (asset, gameObject.FindChildByName($"Office_{i}").transform.parent.gameObject);
                else
                    Mapify.LogError($"Failed to find {nameof(VanillaAsset)} {enumName}!");
            }

            #endregion

            #region Pitstops

            GameObject refillStationParent = gameObject.FindChildByName("RefillStations");
            if (refillStationParent != null)
            {
                yield return (VanillaAsset.PitStopStationCoal1, refillStationParent.FindChildByName("CoalServiceStation01"));
                yield return (VanillaAsset.PitStopStationCoal2, refillStationParent.FindChildByName("CoalServiceStation02"));
                yield return (VanillaAsset.PitStopStationWater1, refillStationParent.FindChildByName("WaterServiceStation01"));
                yield return (VanillaAsset.PitStopStationWater2, refillStationParent.FindChildByName("WaterServiceStation02"));
                yield return (VanillaAsset.PitStopStation, refillStationParent.FindChildByName("PitStopStation"));
                yield return (VanillaAsset.RefillMachineDiesel, refillStationParent.FindChildByName("Diesel"));
                yield return (VanillaAsset.RefillMachineSand, refillStationParent.FindChildByName("Sand"));
                yield return (VanillaAsset.RefillMachineOil, refillStationParent.FindChildByName("Oil"));
                yield return (VanillaAsset.RefillMachineWater, refillStationParent.FindChildByName("WaterLocoResourceModule"));
                yield return (VanillaAsset.RefillMachineCoal, refillStationParent.FindChildByName("CoalLocoResourceModule"));
                yield return (VanillaAsset.RefillMachineBodyDamage, refillStationParent.FindChildByName("Body"));
                yield return (VanillaAsset.RefillMachineWheelDamage, refillStationParent.FindChildByName("Wheels"));
                yield return (VanillaAsset.RefillMachineMechanicalPowertrain, refillStationParent.FindChildByName("Mechanical powertrain"));
                yield return (VanillaAsset.RefillMachineElectricalPowertrain, refillStationParent.FindChildByName("Electrical powertrain"));
                yield return (VanillaAsset.ServiceStationMarkerOpen, refillStationParent.FindChildByName("ServiceStationMarker-open"));
                yield return (VanillaAsset.ServiceStationMarkerClosed, refillStationParent.FindChildByName("ServiceStationMarker-closed"));
                yield return (VanillaAsset.CashRegister, refillStationParent.FindChildByName("CashRegisterResourceModules"));
            }
            else
            {
                Mapify.LogCritical("Failed to find RefillStations!");
            }

            #endregion

            #region Stores

            GameObject shopsParent = gameObject.FindChildByName("[ShopLogic]");
            if (shopsParent == null)
            {
                Mapify.LogError("Failed to find [ShopLogic]!");
                yield break;
            }

            foreach (var module in shopsParent.GetComponentsInChildren<ScanItemCashRegisterModule>())
            {
                string itemName = module.sellingItemSpec.name.Replace("_", "");
                if (itemName.StartsWith("Key")) continue;
                if (Enum.TryParse($"StoreItem{itemName}", true, out VanillaAsset asset))
                    yield return (asset, module.gameObject);
                else
                    Mapify.LogError($"Failed to find {nameof(VanillaAsset)} for shop item {itemName}");
            }

            var shop = shopsParent.GetComponentInChildren<Shop>(true);
            if (!shop)
            {
                Mapify.LogError(nameof(GameContentCopier)+": failed to find any shops!");
                yield break;
            }

            foreach (var module in shop.GetComponentsInChildren<ScanItemCashRegisterModule>())
                Object.Destroy(module.gameObject);

            Object.Destroy(shop.gameObject.FindChildByName("PosterAnchor"));

            yield return (VanillaAsset.StoreObject, shop.gameObject);

            foreach (Transform transform in shopsParent.transform.GetChildren())
                Object.Destroy(transform.gameObject);
            yield return ((VanillaAsset)nextIdx++, shopsParent);

            #endregion
        }
    }
}
