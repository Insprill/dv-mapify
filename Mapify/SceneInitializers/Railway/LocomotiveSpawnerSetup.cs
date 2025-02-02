using System.Linq;
using DV;
using Mapify.Editor;
using UnityEngine;

namespace Mapify.SceneInitializers.Railway
{
    public class LocomotiveSpawnerSetup : SceneSetup
    {
        public override void Run()
        {
            foreach (LocomotiveSpawner spawner in Object.FindObjectsOfType<LocomotiveSpawner>()) SetupLocomotiveSpawner(spawner);
        }

        public static void SetupLocomotiveSpawner(LocomotiveSpawner spawner)
        {
            bool wasActive = spawner.gameObject.activeSelf;
            spawner.gameObject.SetActive(false);
            StationLocoSpawner locoSpawner = spawner.gameObject.AddComponent<StationLocoSpawner>();
            locoSpawner.spawnRotationFlipped = spawner.flipOrientation;
            locoSpawner.locoSpawnTrackName = spawner.Track.name;
            locoSpawner.locoTypeGroupsToSpawn = spawner.condensedLocomotiveTypes
                .Select(rollingStockTypes =>
                    new ListTrainCarTypeWrapper(rollingStockTypes.Split(',').Select(rollingStockType =>
                            Globals.G.Types.Liveries.Find(l => l.id == rollingStockType)
                        ).ToList()
                    )
                ).ToList();

            if (!locoSpawner.locoTypeGroupsToSpawn.Any())
            {
                Mapify.LogError($"{nameof(LocomotiveSpawnerSetup)} locoTypeGroupsToSpawn is empty. {nameof(spawner.condensedLocomotiveTypes)}: {spawner.condensedLocomotiveTypes}");
            }

            spawner.gameObject.SetActive(wasActive);
        }
    }
}
