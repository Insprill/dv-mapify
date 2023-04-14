using Mapify.Editor.Utils;
using UnityEngine;

namespace Mapify.Editor
{
    [ExecuteInEditMode]
    public class SplitVisualizer : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        private float drawHeight;
#pragma warning restore CS0649

        internal Renderer[] renderers;

        private void Awake()
        {
            renderers = FindObjectsOfType<Renderer>();
        }

        public void OnDrawGizmos()
        {
            (float minX, float minZ, float maxX, float maxZ) = renderers.GroupedBounds();

            int chunkSize = EditorAssets.FindAsset<MapInfo>().chunkSize;
            Vector3 chunkSizeVector = new Vector3(chunkSize, 0, chunkSize);

            float sceneSizeX = Mathf.CeilToInt(maxX - minX);
            float sceneSizeZ = Mathf.CeilToInt(maxZ - minZ);
            int numChunksX = Mathf.CeilToInt(sceneSizeX / chunkSize);
            int numChunksZ = Mathf.CeilToInt(sceneSizeZ / chunkSize);

            for (int chunkX = 0; chunkX < numChunksX; chunkX++)
            for (int chunkZ = 0; chunkZ < numChunksZ; chunkZ++)
            {
                float chunkMinX = minX + chunkSize * chunkX;
                float chunkMinZ = minZ + chunkSize * chunkZ;
                Vector3 chunkCenter = new Vector3(chunkMinX + chunkSize / 2f, drawHeight, chunkMinZ + chunkSize / 2f);
                Gizmos.DrawWireCube(chunkCenter, chunkSizeVector);
            }
        }
    }
}