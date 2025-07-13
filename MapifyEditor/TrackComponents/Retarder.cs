using UnityEngine;

namespace Mapify.Editor
{
    // A retarder is a device used to reduce the speed of freight cars
    public class Retarder: MonoBehaviour
    {
        [Tooltip("The retarder will activate when the speed of the car is above this limit (km/h)")]
        public float maxSpeedKMH = 10.0f;
        [Tooltip("The maximum brake force of the retarder, in Newtons")]
        public float brakeForce = 50000f;
    }
}
