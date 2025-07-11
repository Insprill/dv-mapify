using UnityEngine;

namespace Mapify.Editor
{
    [RequireComponent(typeof(Track))]
    public class TimedCarSpawner: MonoBehaviour
    {
        [Tooltip("Interval between spawning, in seconds")]
        public float SpawnInterval = 5;
        [Tooltip("What types of rolling stock to spawn")]
        public VanillaRollingStockType[] TrainCarTypes;
        [Tooltip("Enable the handbrake on the spawned car")]
        public bool EnableHandBrakeOnSpawn = false;
    }
}
