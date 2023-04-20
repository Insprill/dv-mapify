using System.Collections.Generic;
using Mapify.Editor.Utils;
using UnityEngine;

namespace Mapify.Editor.StateUpdaters
{
    public class TerrainMapInfoUpdater : BuildUpdater
    {
        protected override void Update(Scenes scenes)
        {
            Terrain[] terrains = scenes.terrainScene.GetAllComponents<Terrain>().Sort();

            foreach (Terrain terrain in terrains)
                terrain.tag = "EditorOnly"; // Terrain data is saved separately and rebuilt at runtime

            MapInfo mapInfo = EditorAssets.FindAsset<MapInfo>();
            Terrain first = terrains[0];
            if (mapInfo != null)
            {
                mapInfo.terrainSize = first.terrainData.size.x;
                mapInfo.terrainMaterial = first.materialTemplate;
                mapInfo.terrainHeight = first.transform.position.y;
                mapInfo.terrainCount = terrains.Length;
                mapInfo.worldSize = CalculateWorldSize(terrains);
                mapInfo.terrainPixelError = first.heightmapPixelError;
                mapInfo.terrainBasemapDistance = first.basemapDistance;
                mapInfo.terrainDrawInstanced = first.drawInstanced;
            }
        }

        private static float CalculateWorldSize(IEnumerable<Terrain> terrains)
        {
            float maxX = 0f;
            float maxZ = 0f;

            foreach (Terrain terrain in terrains)
            {
                Vector3 terrainSize = terrain.terrainData.size;
                Vector3 position = terrain.transform.position;
                float terrainMaxX = position.x + terrainSize.x;
                float terrainMaxZ = position.z + terrainSize.z;
                if (terrainMaxX > maxX) maxX = terrainMaxX;
                if (terrainMaxZ > maxZ) maxZ = terrainMaxZ;
            }

            return Mathf.Max(maxX, maxZ);
        }
    }
}
