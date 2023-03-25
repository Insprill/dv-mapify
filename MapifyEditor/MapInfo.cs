using UnityEngine;

namespace Mapify.Editor
{
    [CreateAssetMenu(menuName = "Mapify/MapInfo")]
    public class MapInfo : ScriptableObject
    {
        public string mapName = "My Custom Map";
        public float waterLevel;
        public float worldSize = 16384f;
        public Vector3 defaultSpawnPosition;
        public Vector3 defaultSpawnRotation;
        [HideInNormalInspector]
        public int terrainCount;
    }
}
