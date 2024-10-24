#if UNITY_EDITOR
using System.Collections.Generic;
using Mapify.Editor.Utils;
using UnityEngine;

namespace Mapify.Editor
{
    public static class MapRenderer
    {
        private const int TEXTURE_SIZE = 2048;

        public static void RenderMapFromTerrain(Terrain[] terrains, MapInfo mapInfo)
        {
            Texture2D combinedHeightmap = CreateHeightmap(terrains, mapInfo);
            Texture2D scaledHeightmap = Resize(combinedHeightmap, TEXTURE_SIZE, TEXTURE_SIZE);
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
            Color[] colors = new Color[terrainWidth * terrainWidth];
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
                    float waterLevel = worldHeight / mapInfo.waterLevel;
                    float terrainLevel = worldHeight / (terrainY + terrainHeight);
                    Color color = worldHeight <= mapInfo.waterLevel
                        ? mapInfo.waterColor.Evaluate(float.IsNaN(waterLevel) ? 0.0f : waterLevel)
                        : mapInfo.terrainColor.Evaluate(float.IsNaN(terrainLevel) ? 0.0f : terrainLevel);
                    colors[y * terrainWidth + x] = color;
                }

                combinedTexture.SetPixels(currentX, currentY, terrainWidth, terrainWidth, colors);

                currentX += terrainWidth;

                if (currentX + terrainWidth <= totalWidth)
                    continue;

                currentX = 0;
                currentY += terrainWidth;
            }

            combinedTexture.Apply();

            return combinedTexture;
        }

        private static Texture2D Resize(Texture2D source, int width, int height)
        {
            RenderTexture rt = RenderTexture.GetTemporary(width, height, 0);
            Graphics.Blit(source, rt, new Material(Shader.Find("Hidden/BlitCopy")));

            Texture2D result = new Texture2D(width, height, TextureFormat.RGBA32, false);
            result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            result.Apply();

            RenderTexture.ReleaseTemporary(rt);
            return result;
        }

        public static void RenderMapFromImage(MapInfo mapInfo)
        {
            Texture2D scaledMapImage = Resize(mapInfo.fixedMapImage, TEXTURE_SIZE, TEXTURE_SIZE);
            mapInfo.mapTextureSerialized = scaledMapImage.EncodeToJPG();
            mapInfo.mapTextureSize = new[] { scaledMapImage.width, scaledMapImage.height };
        }
    }
}
#endif
