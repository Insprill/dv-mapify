using System.Collections.Generic;
using DV;
using DV.RenderTextureSystem;
using Mapify.Utils;
using UnityEngine;

namespace Mapify.SceneInitializers
{
    public static class GameContentSceneInitializer
    {
        public static void SceneLoaded()
        {
            SetupGameScene();
        }

        private static void SetupGameScene()
        {
            Main.Logger.Log("Creating SaveLoadController");
            new GameObject("[LicensesAndGarages]").AddComponent<SaveLoadController>();
            Main.Logger.Log("Creating CarSpawner");
            new GameObject("[CarSpawner]").WithComponent<CarSpawner>().WithComponent<CarSpawnerOriginShiftHandler>();
            Main.Logger.Log("Creating ItemDisablerGrid");
            new GameObject("[JobLogicController]").AddComponent<LogicController>();
            Main.Logger.Log("Creating ItemDisablerGrid");
            new GameObject("[DerailAndDamageObserver]").AddComponent<DerailAndDamageObserver>();
            Main.Logger.Log("Creating StorageLogic");
            GameObject storageLogic = new GameObject("[StorageLogic]");
            storageLogic.NewChild("StorageWorld").WithComponentT<StorageBase>().storageType = StorageType.World;
            storageLogic.NewChild("StorageLostAndFound").WithComponentT<StorageBase>().storageType = StorageType.LostAndFound;
            storageLogic.NewChild("StorageInventory").WithComponentT<StorageBase>().storageType = StorageType.Inventory;
            storageLogic.NewChild("StorageBelt").WithComponentT<StorageBase>().storageType = StorageType.Belt;
            storageLogic.AddComponent<StorageController>(); // Must be added after all StorageBase's
            Main.Logger.Log("Creating ItemDisablerGrid");
            new GameObject("[ItemDisablerGrid]").AddComponent<ItemDisablerGrid>();
            Main.Logger.Log("Creating ShopLogic");
            GlobalShopController globalShopController = new GameObject("[ShopLogic]").AddComponent<GlobalShopController>();
            globalShopController.globalShopList = new List<Shop>();
            globalShopController.shopItemsData = new List<ShopItemData>();
            Main.Logger.Log("Creating RenderTextureSystem");
            new GameObject("[RenderTextureSystem]").AddComponent<RenderTextureSystem>();
        }
    }
}
