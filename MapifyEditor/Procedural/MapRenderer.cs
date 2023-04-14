using System.Collections.Generic;
using Mapify.Editor.Utils;
using UnityEngine;

namespace Mapify.Editor.Procedural
{
    public static class MapRenderer
    {
        private const int TEXTURE_SIZE = 2048;

        public static void RenderMap(Terrain[] terrains, MapInfo mapInfo)
        {
            Texture2D combinedHeightmap = CreateHeightmap(terrains, mapInfo);
            Texture2D scaledHeightmap = combinedHeightmap.Resize(TEXTURE_SIZE, TEXTURE_SIZE, FilterMode.Bilinear);
            mapInfo.mapTextureSerialized = scaledHeightmap.EncodeToJPG();
            mapInfo.mapTextureSize = new[] { scaledHeightmap.width, scaledHeightmap.height };
        }

        private static Texture2D CreateHeightmap(IReadOnlyList<Terrain> terrains, MapInfo mapInfo)
        {
            int terrainCount = terrains.Count;

            int heightmapResolution = terrains[0].terrainData.heightmapResolution;
            int terrainWidth = heightmapResolution - 1;
            int totalWidth = terrainWidth * Mathf.CeilToInt(Mathf.Sqrt(terrainCount));

            Texture2D combinedTexture = new Texture2D(totalWidth, totalWidth, TextureFormat.RGBA32, false);

            int currentX = 0;
            int currentY = 0;
            for (int i = 0; i < terrainCount; i++)
            {
                Terrain terrain = terrains[i];
                float terrainY = terrain.transform.position.y;
                TerrainData terrainData = terrain.terrainData;
                float terrainHeight = terrainData.size.y;
                float[,] heightmapData = terrainData.GetHeights(0, 0, heightmapResolution, heightmapResolution);

                for (int y = 0; y < terrainWidth; y++)
                for (int x = 0; x < terrainWidth; x++)
                {
                    float height = heightmapData[y, x];
                    float worldHeight = terrainY + height * terrainHeight;
                    Color color = worldHeight <= mapInfo.waterLevel
                        ? mapInfo.waterColor.Evaluate(worldHeight / mapInfo.waterLevel)
                        : mapInfo.terrainColor.Evaluate(worldHeight / (terrainY + terrainHeight));
                    combinedTexture.SetPixel(currentX + x, currentY + y, color);
                }

                currentX += terrainWidth;

                if (currentX + terrainWidth <= totalWidth)
                    continue;

                currentX = 0;
                currentY += terrainWidth;
            }

            combinedTexture.Apply();

            return combinedTexture;
        }
    }
}
