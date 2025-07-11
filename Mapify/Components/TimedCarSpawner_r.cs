using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DV;
using DV.ThingTypes;
using DV.Utils;
using Mapify.Editor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Mapify.Components
{
    public class TimedCarSpawner_r: MonoBehaviour
    {
        private float SpawnInterval;
        private bool EnableHandBrakeOnSpawn;
        private RailTrack spawnTrack;
        private TrainCarType_v2[] trainCarTypes;

        private void Start()
        {
            var carSpawnerValues = GetComponent<TimedCarSpawner>();
            if (!carSpawnerValues)
            {
                Mapify.LogError($"Can't find {nameof(TimedCarSpawner)} on {gameObject.name}");
                Destroy(this);
                return;
            }

            spawnTrack = GetComponent<RailTrack>();
            SpawnInterval = carSpawnerValues.SpawnInterval;

            var TrainCarIDs = carSpawnerValues.TrainCarTypes.Select(type => Enum.GetName(type.GetType(), type)).ToArray();
            trainCarTypes = TrainCarIDs.Select(carID => Globals.G.types._carTypesById[carID]).ToArray();

            foreach (var trainCarType in trainCarTypes)
            {
                if (!trainCarType.liveries.Any())
                {
                    Mapify.LogError($"trainCarType {trainCarType} has no liveries");
                }
            }

            EnableHandBrakeOnSpawn = carSpawnerValues.EnableHandBrakeOnSpawn;

            StartCoroutine(Spawn());
        }

        private IEnumerator Spawn()
        {
            while (true)
            {
                while (spawnTrack.BogiesOnTrack().Any())
                {
                    yield return null;
                }

                yield return new WaitForSeconds(SpawnInterval);

                while (spawnTrack.BogiesOnTrack().Any())
                {
                    yield return null;
                }

                var nextCar = trainCarTypes[Random.Range(0, trainCarTypes.Length)];
                var nextLivery = nextCar.liveries[Random.Range(0, nextCar.liveries.Count)];

                Mapify.LogDebug($"Spawning {nextCar.id} with livery {nextLivery.id}");

                var trainCarList = SingletonBehaviour<CarSpawner>.Instance.SpawnCarTypesOnTrack(
                    new List<TrainCarLivery>{ nextLivery },
                    null,
                    spawnTrack,
                    true,
                    EnableHandBrakeOnSpawn
                );

                if (trainCarList == null || !trainCarList.Any())
                {
                    Mapify.LogError($"Car spawning failed! Is the track long enough to fit the traincar?");
                }
                else
                {
                    Mapify.LogDebug($"Spawn result:");
                    foreach (var spawnedCar in trainCarList)
                    {
                        Mapify.LogDebug($"{spawnedCar.carType} {spawnedCar.carLivery.id}");
                    }
                }
            }
        }
    }
}
