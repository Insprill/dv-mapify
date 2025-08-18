using System;
using System.Collections.Generic;
using System.Linq;
using DV.CashRegister;
using DV.Optimizers;
using DV.Printers;
using DV.Shops;
using DV.Utils;
using Mapify.Editor;
using Mapify.Editor.Utils;
using Mapify.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Mapify.SceneInitializers.GameContent
{
    public class StoreSetup : SceneSetup
    {
        private const float STORE_Y_ROT_OFFSET = 90;

        public override void Run()
        {
            GlobalShopController gsc = SingletonBehaviour<GlobalShopController>.Instance;
            gsc.globalShopList = new List<Shop>();

            var allItemTypes = Enum.GetValues(typeof(Editor.ItemType)).Cast<Editor.ItemType>().ToArray();

            foreach (Store store in Object.FindObjectsOfType<Store>())
            {
                Transform shopTransform = AssetCopier.Instantiate(VanillaAsset.StoreObject, active:false).transform;

                var optimizer = shopTransform.GetComponent<PlayerDistanceMultipleGameObjectsOptimizer>();
                optimizer.gameObjectsToDisable = new List<GameObject>();

                foreach (Transform child in store.cashRegister.GetChildren())
                {
                    Renderer r = child.GetComponent<Renderer>();
                    if (r != null) Object.Destroy(r);
                    child.SetParent(shopTransform, false);
                }

                store.itemSpawnReference.localPosition = store.itemSpawnReference.localPosition.SwapAndInvertXZ();

                Transform meshTransform = AssetCopier.Instantiate(VanillaAsset.StoreMesh, false).transform;
                shopTransform.SetParent(meshTransform, false);
                shopTransform.localPosition += store.cashRegister.localPosition;
                shopTransform.eulerAngles = shopTransform.eulerAngles.AddY(STORE_Y_ROT_OFFSET);

                Shop shop = shopTransform.GetComponent<Shop>();
                shop.itemSpawnTransform = store.itemSpawnReference;
                gsc.globalShopList.Add(shop);

                var itemTypes = store.SpecifyItems ? store.itemTypes : allItemTypes;

                shop.scanItemResourceModules = new ScanItemCashRegisterModule[itemTypes.Length];
                for (int i = 0; i < itemTypes.Length; i++)
                {
                    Transform t = AssetCopier.Instantiate((VanillaAsset)itemTypes[i]).transform;
                    t.SetParent(store.moduleReference, false);
                    store.PositionThing(store.moduleReference, t, i);
                    shop.scanItemResourceModules[i] = t.GetComponent<ScanItemCashRegisterModule>();
                    shop.scanItemResourceModules[i].shop = shop;
                    optimizer.gameObjectsToDisable.Add(t.gameObject);
                }

                CashRegisterWithModules cashRegister = shopTransform.GetComponentInChildren<CashRegisterWithModules>();
                cashRegister.registerModules = shop.scanItemResourceModules.Cast<CashRegisterModule>().ToArray();
                optimizer.gameObjectsToDisable.Add(cashRegister.gameObject);
                cashRegister.GetComponent<PrinterController>().spawnAnchor = store.itemSpawnReference;

                optimizer.gameObjectsToDisable.Add(shopTransform.FindChildByName("ScannerAnchor").gameObject);

                store.gameObject.Replace(meshTransform.gameObject).SetActive(true);

                shopTransform.gameObject.SetActive(true);
            }

            gsc.gameObject.SetActive(true);
        }
    }
}
