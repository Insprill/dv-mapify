using UnityEngine;

namespace Mapify.Editor
{
    [CreateAssetMenu(menuName = "Mapify/MapInfo")]
    public class MapInfo : ScriptableObject
    {
        public string mapName = "My Custom Map";
        public float waterLevel;
        public Vector3 defaultSpawnPosition;
        public Vector3 defaultSpawnRotation;
        [Header("Terrain Streaming")]
        [Tooltip("How many terrain chunks to keep loaded around the player")]
        public byte terrainLoadingRingSize = 2;
        [Header("World Streaming")]
        [Tooltip("The size of each streaming chunk")]
        [Range(128, 1024)]
        public ushort chunkSize = 512;
        [Tooltip("How many chunks to keep loaded around the player")]
        public byte worldLoadingRingSize = 2;

        [HideInNormalInspector]
        public float worldSize;
        [HideInNormalInspector]
        public float terrainSize;
        [HideInNormalInspector]
        public float terrainHeight;
        [HideInNormalInspector]
        public int terrainCount;
        [HideInNormalInspector]
        public Material terrainMaterial;
        [HideInNormalInspector]
        public float terrainPixelError;
        [HideInNormalInspector]
        public bool terrainDrawInstanced;
        [HideInNormalInspector]
        public float terrainBasemapDistance;
        [HideInNormalInspector]
        public string sceneSplitData;
    }
}
