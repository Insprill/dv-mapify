#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Mapify.Editor.Utils;

namespace Mapify.Editor.Validators
{
    public class StoreValidator : Validator
    {
        protected override IEnumerator<Result> Validate(Scenes scenes)
        {
            Store[] stores = scenes.gameContentScene.GetAllComponents<Store>();
            if (stores.Length == 0)
                yield return Result.Warning("No stores found! Player's may not be able to acquire essential items for progressing");

            foreach (Store store in stores)
            {
                if (store.SpecifyItems && store.itemTypes.Length == 0)
                    yield return Result.Error($"Stores must have at least one item type when {nameof(Store.SpecifyItems)} is true.", store);

                var duplicateItemTypes = store.itemTypes.GroupBy(e => e)
                    .Where(g => g.Count() > 1)
                    .Select(g => new { Item = g.Key, Count = g.Count() })
                    .Distinct();
                foreach (var duplicate in duplicateItemTypes)
                    yield return Result.Error($"Stores can only have one of each item type! Found {duplicate.Count} {duplicate.Item}'s", store);

                if (store.cashRegister == null)
                    yield return Result.Error("Store is missing a cash register", store);

                if (store.cashRegister.parent != store.transform)
                    yield return Result.Error("Store's cash register must be a child of the store", store);

                if (store.moduleReference == null)
                    yield return Result.Error("Store is missing a module reference", store);

                if (store.moduleReference.parent != store.cashRegister)
                    yield return Result.Error("Store' module reference must be a child of the cash register", store);

                if (store.itemSpawnReference == null)
                    yield return Result.Error("Store is missing a module reference", store);

                if (store.itemSpawnReference.parent != store.cashRegister)
                    yield return Result.Error("Store's item spawn reference must be a child of the cash register", store);
            }
        }
    }
}
#endif
