using System;
using System.Linq;
using DV.Shops;
using HarmonyLib;

namespace Mapify.Patches
{
#if DEBUG
    [HarmonyPatch(typeof(GlobalShopController), nameof(GlobalShopController.InitializeShopData))]
    public class GlobalShopController_InitializeShopData_Patch
    {
        private static void Postfix(GlobalShopController __instance)
        {
            VerifyItemTypeEnum(__instance);
        }

        /// <summary>
        /// Verify that the ItemType enum is up to date.
        /// </summary>
        private static void VerifyItemTypeEnum(GlobalShopController __instance)
        {
            var inGameItems = __instance.shopItemsData.Select(dat => dat.item.itemPrefabName.ToLower().Replace("_", "")).ToArray();

            var inModItems = Enum.GetValues(typeof(Editor.ItemType))
                .Cast<Editor.ItemType>()
                .Select(itemEnum => itemEnum.ToString().ToLower())
                .ToArray();

            bool verifiedSuccessfully = true;

            foreach (var inModItem in inModItems)
            {
                if(inGameItems.Contains(inModItem)) continue;

                Mapify.LogError($"{nameof(ItemType)} '{inModItem}' does not exist in-game");
                verifiedSuccessfully = false;
            }

            foreach (var inGameItem in inGameItems)
            {
                if(inModItems.Contains(inGameItem)) continue;

                // the keys are intentionally left out
                if(inGameItem.StartsWith("key")) continue;

                Mapify.LogWarning($"Item '{inGameItem}' does not exist in enum {nameof(ItemType)}");
            }

            if(verifiedSuccessfully) return;

            Mapify.LogDebug("Items in game:");
            foreach (var inGameItem in inGameItems)
            {
                Mapify.LogDebug(inGameItem);
            }
        }
    }
#endif
}
