using UnityEngine;

namespace Mapify.Editor
{
    [CreateAssetMenu(menuName = "Mapify/MapInfo")]
    public class MapInfo : ScriptableObject
    {
        [Header("Map Information")]
        [Tooltip("The display name of the map")]
        public new string name = "My Custom Map";
        [Tooltip("The version of your map, in semver format (https://semver.org/)")]
        public string version = "0.1.0";
        [Tooltip("The home page of your mod, most likely being the Nexus Mods page")]
        public string homePage = "https://www.nexusmods.com/derailvalley/mods/MOD-ID-HERE";

        [Header("Loading Gauge")]
        [Tooltip("The height of the loading gauge, in meters")]
        public float loadingGaugeHeight = 5;
        [Tooltip("The width of the loading gauge, in meters")]
        public float loadingGaugeWidth = 4;

        [Header("World")]
        [Min(-1)]
        [Tooltip("The height at which water will appear")]
        public float waterLevel = -1;
        [Tooltip("The player's initial spawn position")]
        public Vector3 defaultSpawnPosition;
        [Tooltip("The player's initial spawn rotation")]
        public Vector3 defaultSpawnRotation;
        [Tooltip("The closest distance, in meters, the player can get to the edges of the map.")]
        public float worldBoundaryMargin = 5.0f;

        [Header("Procedural Maps")]
        [Tooltip("Use an image as map instead of rendering the terrain on the map")]
        public bool useFixedMapImage = false;
        [Tooltip("If useFixedMapImage is true, this image will be the map")]
        public Texture2D fixedMapImage = null;
        [Tooltip("How large tracks should be on the map")]
        public float trackWidth = 5;
        [Tooltip("The color of tracks on the map")]
        public Color trackColor = new Color32(206, 195, 148, 255);
        [Tooltip("How large of a border tracks should have on the map")]
        public float trackBackgroundWidth = 17;
        [Tooltip("How large of a border tracks should have on the map")]
        public Vector2 trackBackgroundOffset = new Vector2(0, -4);
        [Tooltip("The color of track borders on the map")]
        public Color trackBackgroundColor = new Color32(57, 48, 33, 255);
        [Tooltip("The color of water on the map. 0% represents the lowest your terrain can go, and 100% represents water level")]
        public Gradient waterColor = new Gradient {
            colorKeys = new[] {
                new GradientColorKey(new Color32(21, 51, 42, 255), 0.0f),
                new GradientColorKey(new Color32(40, 96, 80, 255), 1.0f)
            },
            alphaKeys = new[] {
                new GradientAlphaKey(1.0f, 0.0f),
                new GradientAlphaKey(1.0f, 1.0f)
            }
        };
        [Tooltip("The color of terrain on the map. 0% represents water level, and 100% represents the highest your terrain can go")]
        public Gradient terrainColor = new Gradient {
            colorKeys = new[] {
                new GradientColorKey(new Color32(86, 95, 66, 255), 0.0f),
                new GradientColorKey(new Color32(178, 175, 136, 255), 1.0f)
            },
            alphaKeys = new[] {
                new GradientAlphaKey(1.0f, 0.0f),
                new GradientAlphaKey(1.0f, 1.0f)
            }
        };

        [Tooltip("Show the names of stations on the map instead of the station IDs")]
        public bool showStationNamesOnMap;

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

        [HideInInspector]
        public string mapifyVersion;
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
        [HideInInspector]
        [SerializeField]
        public byte[] mapTextureSerialized;
        [HideInInspector]
        [SerializeField]
        public int[] mapTextureSize;

        #endregion
    }
}
