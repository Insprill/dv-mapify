using UnityEngine;

namespace Mapify.Editor
{
    [CreateAssetMenu(menuName = "Mapify/MapInfo")]
    public class MapInfo : ScriptableObject
    {
        [Header("Map Information")]
        [Tooltip("The display name of the map")]
        public string mapName = "My Custom Map";
        [Tooltip("The height at which water will appear")]
        public float waterLevel;
        [Tooltip("The player's initial spawn position")]
        public Vector3 defaultSpawnPosition;
        [Tooltip("The player's initial spawn rotation")]
        public Vector3 defaultSpawnRotation;

        [Header("Terrain Streaming")]
        [Tooltip("How many terrain chunks to keep loaded around the player")]
        public byte terrainLoadingRingSize = 2;

        [Header("World Streaming")]
        [Tooltip("The size of each streaming chunk")]
        [Range(128, 2048)]
        public ushort chunkSize = 512;
        [Tooltip("How many chunks to keep loaded around the player")]
        public byte worldLoadingRingSize = 2;

        #region Internal Stuff

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

        #endregion
    }
}
