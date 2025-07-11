using System.Linq;
using DV.Utils;
using UnityEngine;

namespace Mapify.Components
{
    public class CarDeleter_r: MonoBehaviour
    {
        private RailTrack railTrack;

        private void Start()
        {
            railTrack = GetComponent<RailTrack>();
        }

        private void Update()
        {
            if(!railTrack.BogiesOnTrack().Any()) return;

            var carToDelete = railTrack.BogiesOnTrack().Select(bogie => bogie._car).First();

            Mapify.LogDebug($"{nameof(CarDeleter_r)}: Deleting {carToDelete.name}");

            //copied from DV.CommsRadioCarDeleter.OnUse
            SingletonBehaviour<CarSpawner>.Instance.DeleteCar(carToDelete);
            SingletonBehaviour<UnusedTrainCarDeleter>.Instance.ClearInvalidCarReferencesAfterManualDelete();
            if (carToDelete != null)
            {
                carToDelete.gameObject.SetActive(false);
                carToDelete.interior.gameObject.SetActive(false);
            }
        }
    }
}
