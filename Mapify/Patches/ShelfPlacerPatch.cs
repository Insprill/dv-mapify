using DV.Shops;
using HarmonyLib;
using Mapify.Map;

namespace Mapify.Patches
{
    [HarmonyPatch(typeof(ShelfPlacer), nameof(ShelfPlacer.TryPlaceOnAnyShelf))]
    public class ShelfPlacer_TryPlaceOnAnyShelf_Patch
    {
        private static void Postfix(ShelfItem item, bool __result)
        {
            if(Maps.IsDefaultMap || !__result) return;
            item.gameObject.SetActive(true);
        }
    }
}
