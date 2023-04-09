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

        private static IEnumerator<(VanillaAsset, GameObject)> ToSave(GameObject gameObject)
        {
            if (gameObject.name != "[origin shift content]")
                yield break;

            yield return (VanillaAsset.Water, gameObject.FindChildByName("water"));

            #region Stations

            yield return (VanillaAsset.CareerManager, gameObject.FindChildByName("CareerManager"));
            yield return (VanillaAsset.JobValidator, gameObject.FindChildByName("JobValidator"));
            yield return (VanillaAsset.TrashCan, gameObject.FindChildByName("JobTrashBin"));
            yield return (VanillaAsset.Dumpster, gameObject.FindChildByName("ItemDumpster"));
            yield return (VanillaAsset.LostAndFoundShed, gameObject.FindChildByName("OldShed"));
            yield return (VanillaAsset.WarehouseMachine, gameObject.FindChildByName("WarehouseMachineHMB"));

            for (int i = 1; i <= 7; i++)
            {
                string enumName = $"StationOffice{i}";
                if (Enum.TryParse(enumName, out VanillaAsset asset))
                    yield return (asset, gameObject.FindChildByName($"Office_0{i}"));
                else
                    Main.Logger.Error($"Failed to find {nameof(VanillaAsset)} {enumName}!");
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
            yield return (VanillaAsset.CashRegister, refillStationParent.FindChildByName("CashRegisterResourceModules"));

            #endregion
        }
    }
}
