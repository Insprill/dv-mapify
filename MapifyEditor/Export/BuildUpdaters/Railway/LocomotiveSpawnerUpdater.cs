#if UNITY_EDITOR
using System.Linq;
using Mapify.Editor.Utils;

namespace Mapify.Editor.StateUpdaters
{
    public class LocomotiveSpawnerUpdater : BuildUpdater
    {
        protected override void Update(Scenes scenes)
        {
            foreach (LocomotiveSpawner spawner in scenes.railwayScene.GetAllComponents<LocomotiveSpawner>())
                spawner.condensedLocomotiveTypes = spawner.locomotiveTypesToSpawn.Select(types => string.Join(",", types.rollingStockTypes.Select(type => type.ToString()))).ToList();
        }
    }
}
#endif
