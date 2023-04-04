using System;
using System.Collections.Generic;
using Mapify.Editor;
using Mapify.Utils;
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

            return gameObjects;
        }
    }
}
