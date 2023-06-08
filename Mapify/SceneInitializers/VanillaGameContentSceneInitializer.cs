using System;
using System.Collections.Generic;
using DV.Shops;
using Mapify.Editor;
using Mapify.Editor.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Mapify.SceneInitializers
{
    public static class VanillaGameContentSceneInitializer
    {
        public static void SceneLoaded(Scene scene)
        {
            AssetCopier.CopyDefaultAssets(scene, ToSave);
        }

        private static IEnumerator<(VanillaAsset, GameObject)> ToSave(GameObject gameObject)
        {
            switch (gameObject.name)
            {
                // Todo: add these
                case "[LicensesAndGarages]":
                    yield return (VanillaAsset.LicensesAndGarages, gameObject);
                    yield break;
                case "[ItemDisablerGrid]":
                    yield return (VanillaAsset.ItemDisablerGrid, gameObject);
                    yield break;
                case "[JobLogicController]":
                    yield return (VanillaAsset.JobLogicController, gameObject);
                    yield break;
                case "[StorageLogic]":
                    yield return (VanillaAsset.StorageLogic, gameObject);
                    yield break;
                case "[ShopLogic]":
                    yield return (VanillaAsset.ShopLogic, gameObject);
                    yield break;
            }

            if (gameObject.name != "[origin shift content]")
                yield break;

            #region Misc

            yield return (VanillaAsset.Water, gameObject.FindChildByName("water"));
            yield return (VanillaAsset.PlayerHouse, gameObject.FindChildByName("PlayerHouse01"));

            #endregion

            #region Stations

            yield return (VanillaAsset.CareerManager, GameObject.Instantiate(gameObject.FindChildByName("CareerManager")));
            yield return (VanillaAsset.JobValidator, GameObject.Instantiate(gameObject.FindChildByName("JobValidator")));
            yield return (VanillaAsset.TrashCan, GameObject.Instantiate(gameObject.FindChildByName("JobTrashBin")));
            yield return (VanillaAsset.Dumpster, gameObject.FindChildByName("ItemDumpster"));
            yield return (VanillaAsset.LostAndFoundShed, gameObject.FindChildByName("OldShed"));
            yield return (VanillaAsset.WarehouseMachine, gameObject.FindChildByName("WarehouseMachineHMB"));

            for (int i = 1; i <= 7; i++)
            {
                string enumName = $"StationOffice{i}";
                if (Enum.TryParse(enumName, out VanillaAsset asset))
                    yield return (asset, gameObject.FindChildByName($"Office_0{i}"));
                else
                    Main.LogError($"Failed to find {nameof(VanillaAsset)} {enumName}!");
            }

            #endregion

            #region Pitstops

            GameObject refillStationParent = gameObject.FindChildByName("RefillStations");

            yield return (VanillaAsset.PitStopStation, refillStationParent.FindChildByName("PitStopStation"));
            yield return (VanillaAsset.RefillMachineFuel, refillStationParent.FindChildByName("Fuel"));
            yield return (VanillaAsset.RefillMachineSand, refillStationParent.FindChildByName("Sand"));
            yield return (VanillaAsset.RefillMachineOil, refillStationParent.FindChildByName("Oil"));
            yield return (VanillaAsset.RefillMachineWater, refillStationParent.FindChildByName("Tender water"));
            yield return (VanillaAsset.RefillMachineCoal, refillStationParent.FindChildByName("Coal"));
            yield return (VanillaAsset.RefillMachineCarDamage, refillStationParent.FindChildByName("Body repair"));
            yield return (VanillaAsset.RefillMachineWheelDamage, refillStationParent.FindChildByName("Wheels repair"));
            yield return (VanillaAsset.RefillMachineEngineDamage, refillStationParent.FindChildByName("Engine repair"));
            yield return (VanillaAsset.ServiceStationMarkerOpen, refillStationParent.FindChildByName("ServiceStationMarker-open"));
            yield return (VanillaAsset.ServiceStationMarkerClosed, refillStationParent.FindChildByName("ServiceStationMarker-closed"));
            yield return (VanillaAsset.CashRegister, refillStationParent.FindChildByName("CashRegisterWithModules"));

            #endregion

            #region Stores

            GameObject shopsParent = gameObject.FindChildByName("Shops");

            foreach (ScanItemCashRegisterModule module in shopsParent.GetComponentsInChildren<ScanItemCashRegisterModule>())
            {
                string itemName = module.sellingItemSpec.name.Replace("_", "");
                if (itemName.StartsWith("Key")) continue;
                if (VanillaAsset.TryParse($"StoreItem{itemName}", true, out VanillaAsset asset))
                    yield return (asset, module.gameObject);
                else
                    Main.LogError($"Failed to find VanillaAsset for {itemName}");
            }

            GameObject shop = shopsParent.FindChildByName("[ItemShop] Harbor");
            foreach (ScanItemCashRegisterModule module in shop.GetComponentsInChildren<ScanItemCashRegisterModule>())
                Object.Destroy(module.gameObject);
            Object.Destroy(shop.FindChildByName("Stopwatch"));

            yield return (VanillaAsset.Store, shop);

            #endregion
        }
    }
}
