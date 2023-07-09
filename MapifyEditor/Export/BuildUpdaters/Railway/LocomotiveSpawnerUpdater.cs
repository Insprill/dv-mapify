#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Mapify.Editor.Utils;

namespace Mapify.Editor.StateUpdaters
{
    public class LocomotiveSpawnerUpdater : BuildUpdater
    {
        protected override void Update(Scenes scenes)
        {
            IEnumerable<LocomotiveSpawner> spawners = new[] {
                scenes.railwayScene,
                scenes.gameContentScene
            }.SelectMany(s => s.GetAllComponents<LocomotiveSpawner>());

            foreach (LocomotiveSpawner spawner in spawners)
                spawner.condensedLocomotiveTypes = spawner.CondenseLocomotiveTypes().ToArray();
        }
    }
}
#endif
