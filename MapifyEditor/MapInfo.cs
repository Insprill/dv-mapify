using UnityEngine;
using UnityEngine.Serialization;

namespace Mapify.Editor
{
    [CreateAssetMenu(menuName = "Mapify/MapInfo")]
    public class MapInfo : ScriptableObject
    {
        public string mapName = "My Custom Map";
        [FormerlySerializedAs("waterHeight")]
        public float waterLevel;
        public Vector3 defaultSpawnPosition;
        public Vector3 defaultSpawnRotation;
        [HideInNormalInspector]
        public float worldSize;
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
    }
}
