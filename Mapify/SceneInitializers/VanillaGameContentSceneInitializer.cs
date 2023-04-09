using System;
using System.Collections.Generic;
using Mapify.Editor;
using Mapify.Editor.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mapify.SceneInitializers
{
    public static class VanillaGameContentSceneInitializer
    {
        public static void SceneLoaded(Scene scene)
        {
            AssetCopier.CopyDefaultAssets(scene, ToSave);
        }

        private static Dictionary<VanillaAsset, GameObject> ToSave(GameObject gameObject)
        {
            Dictionary<VanillaAsset, GameObject> gameObjects = new Dictionary<VanillaAsset, GameObject>(3);
            if (gameObject.name != "[origin shift content]")
                return gameObjects;

            GameObject water = gameObject.FindChildByName("water");
            gameObjects.Add(VanillaAsset.Water, water);

            #region Stations

            GameObject careerManager = gameObject.FindChildByName("CareerManager");
            gameObjects.Add(VanillaAsset.CareerManager, careerManager);

            GameObject jobValidator = gameObject.FindChildByName("JobValidator");
            gameObjects.Add(VanillaAsset.JobValidator, jobValidator);

            GameObject trashCan = gameObject.FindChildByName("JobTrashBin");
            gameObjects.Add(VanillaAsset.TrashCan, trashCan);

            GameObject dumpster = gameObject.FindChildByName("ItemDumpster");
            gameObjects.Add(VanillaAsset.Dumpster, dumpster);

            GameObject shed = gameObject.FindChildByName("OldShed");
            gameObjects.Add(VanillaAsset.LostAndFoundShed, shed);

            GameObject warehouseMachine = gameObject.FindChildByName("WarehouseMachineHMB");
            gameObjects.Add(VanillaAsset.WarehouseMachine, warehouseMachine);

            for (int i = 1; i <= 7; i++)
            {
                GameObject office = gameObject.FindChildByName($"Office_0{i}");
                string enumName = $"StationOffice{i}";
                if (Enum.TryParse(enumName, out VanillaAsset asset))
                    gameObjects.Add(asset, office);
                else
                    Main.Logger.Error($"Failed to find {nameof(VanillaAsset)} {enumName}!");
            }

            #endregion

            #region Pitstops

            GameObject refillStationParent = gameObject.FindChildByName("RefillStations");

            GameObject pitStopStation = refillStationParent.FindChildByName("PitStopStation");
            gameObjects.Add(VanillaAsset.PitStopStation, pitStopStation);

            GameObject fuel = refillStationParent.FindChildByName("Fuel");
            gameObjects.Add(VanillaAsset.RefillMachineFuel, fuel);

            GameObject sand = refillStationParent.FindChildByName("Sand");
            gameObjects.Add(VanillaAsset.RefillMachineSand, sand);

            GameObject oil = refillStationParent.FindChildByName("Oil");
            gameObjects.Add(VanillaAsset.RefillMachineOil, oil);

            GameObject tenderWater = refillStationParent.FindChildByName("Tender water");
            gameObjects.Add(VanillaAsset.RefillMachineWater, tenderWater);

            GameObject coal = refillStationParent.FindChildByName("Coal");
            gameObjects.Add(VanillaAsset.RefillMachineCoal, coal);

            GameObject body = refillStationParent.FindChildByName("Body repair");
            gameObjects.Add(VanillaAsset.RefillMachineCarDamage, body);

            GameObject wheels = refillStationParent.FindChildByName("Wheels repair");
            gameObjects.Add(VanillaAsset.RefillMachineWheelDamage, wheels);

            GameObject engine = refillStationParent.FindChildByName("Engine repair");
            gameObjects.Add(VanillaAsset.RefillMachineEngineDamage, engine);

            GameObject markerOpen = refillStationParent.FindChildByName("ServiceStationMarker-open");
            gameObjects.Add(VanillaAsset.ServiceStationMarkerOpen, markerOpen);

            GameObject markerClosed = refillStationParent.FindChildByName("ServiceStationMarker-closed");
            gameObjects.Add(VanillaAsset.ServiceStationMarkerClosed, markerClosed);

            GameObject pitstopCashRegister = refillStationParent.FindChildByName("CashRegisterResourceModules");
            gameObjects.Add(VanillaAsset.CashRegister, pitstopCashRegister);

            #endregion

            return gameObjects;
        }
    }
}
