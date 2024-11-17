using System.Linq;
using UnityEngine;

namespace Mapify.Components
{
    public class Retarder: MonoBehaviour
    {
        private float maxSpeed; // meter per second
        private RailTrack railTrack;
        private float brakeForce; //Newton

        private bool hasBeenSetup = false;

        public void Setup(float maxSpeed_, RailTrack railTrack_, float brakeForce_)
        {
            maxSpeed = maxSpeed_;
            railTrack = railTrack_;
            brakeForce = brakeForce_;

            hasBeenSetup = true;
        }

        private void Awake()
        {
            if (!hasBeenSetup)
            {
                Mapify.LogError($"{nameof(Retarder)} on {gameObject.name} has not been setup yet");
                Destroy(this);
            }
        }

        private void FixedUpdate()
        {
            foreach (var car in railTrack.onTrackBogies.Select(bogie => bogie._car))
            {
                if(car.GetAbsSpeed() <= maxSpeed) { return; }

                var forwardSpeed = car.GetForwardSpeed();
                var force3D = car.transform.forward * (-brakeForce * Mathf.Sign(forwardSpeed));
                car.rb.AddForce(force3D);

                Mapify.LogDebugExtreme(() => $"{nameof(Retarder)} force {force3D}");
                Mapify.LogDebugExtreme(() => $"{nameof(Retarder)} forwardSpeed {forwardSpeed}");
            }
        }
    }
}
