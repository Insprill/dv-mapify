using System.Collections.Generic;
using System.Linq;
using Mapify.Editor;
using Mapify.Editor.Utils;
using Mapify.Editor.Validators;

namespace MapifyEditor.Export.Validators
{
    public class LocomotiveSpawnerValidator : Validator
    {
        protected override IEnumerator<Result> Validate(Scenes scenes)
        {
            foreach (LocomotiveSpawner spawner in scenes.railwayScene.GetAllComponents<LocomotiveSpawner>())
                if (spawner.locomotiveTypesToSpawn.Count == 0)
                    yield return Result.Error("Locomotive spawners must have at least one group to spawn!", spawner);
                else if (spawner.locomotiveTypesToSpawn.Any(group => group.rollingStockTypes.Count == 0))
                    yield return Result.Error("Locomotive spawner groups must have at least one type to spawn!", spawner);
                else
                    spawner.condensedLocomotiveTypes = spawner.locomotiveTypesToSpawn.Select(types => string.Join(",", types.rollingStockTypes.Select(type => type.ToString()))).ToList();
        }
    }
}
