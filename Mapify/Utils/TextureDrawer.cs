using UnityEngine;

namespace Mapify.Utils
{
    /// <summary>
    ///     Utility class for drawing onto textures.
    /// </summary>
    public class TextureDrawer
    {
        public readonly Texture2D texture;
        private readonly int textureWidth;
        private readonly int textureHeight;
        private readonly Color[] pixelBuffer;

        public TextureDrawer(Texture2D texture)
        {
            this.texture = texture;
            textureWidth = texture.width;
            textureHeight = texture.height;
            pixelBuffer = texture.GetPixels();
        }

        public void DrawLineOnTexture(Vector2 startPoint, Vector2 endPoint, float lineWidth, Color lineColor)
        {
            Vector2 direction = endPoint - startPoint;
            float distance = direction.magnitude;
            direction.Normalize();

            for (int i = 0; i < distance; i++)
            {
                Vector2 point = startPoint + direction * i;
                DrawPixelOnBuffer(point, lineWidth, lineColor);
            }
        }

        private void DrawPixelOnBuffer(Vector2 point, float size, Color color)
        {
            int halfSize = Mathf.CeilToInt(size / 2f);
            for (int x = -halfSize; x <= halfSize; x++)
            for (int y = -halfSize; y <= halfSize; y++)
            {
                int pixelX = Mathf.RoundToInt(point.x + x);
                int pixelY = Mathf.RoundToInt(point.y + y);

                if (pixelX >= 0 && pixelX < textureWidth && pixelY >= 0 && pixelY < textureHeight)
                    pixelBuffer[pixelY * textureWidth + pixelX] = color;
            }
        }

        public void Apply()
        {
            texture.SetPixels(pixelBuffer);
            texture.Apply();
        }
    }
}
