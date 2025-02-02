using System.Linq;
using Mapify.Editor;
using UnityEngine;

namespace Mapify.Components
{
    public class Retarder_r: MonoBehaviour
    {
        private float maxSpeed; // meter per second
        private RailTrack railTrack;
        private float brakeForce; //Newton

        private void Start()
        {
            var retarderValues = GetComponent<Retarder>();
            if (!retarderValues)
            {
                Mapify.LogError($"Can't find {nameof(Retarder)} on {gameObject.name}");
                Destroy(this);
                return;
            }

            railTrack = GetComponent<RailTrack>();

            maxSpeed = retarderValues.maxSpeedKMH / 3.6f; // to m/s
            brakeForce = retarderValues.brakeForce;
        }

        private void FixedUpdate()
        {
            foreach (var car in railTrack.onTrackBogies.Select(bogie => bogie._car))
            {
                if(car.GetAbsSpeed() <= maxSpeed) { return; }

                var forwardSpeed = car.GetForwardSpeed();
                var force3D = car.transform.forward * (-brakeForce * Mathf.Sign(forwardSpeed));
                car.rb.AddForce(force3D);

                Mapify.LogDebugExtreme(() => $"{nameof(Retarder_r)} force {force3D}");
                Mapify.LogDebugExtreme(() => $"{nameof(Retarder_r)} forwardSpeed {forwardSpeed}");
            }
        }
    }
}
