using UnityEngine;

namespace Mapify.Utils
{
    /// <summary>
    ///     Utility class for drawing onto textures.
    /// </summary>
    public class TextureDrawer
    {
        public readonly Texture2D texture;

        public TextureDrawer(Texture2D texture)
        {
            this.texture = texture;
        }

        public void DrawLineOnTexture(Vector2 startPoint, Vector2 endPoint, float lineWidth, Color lineColor)
        {
            Vector2 direction = endPoint - startPoint;
            float distance = direction.magnitude;
            direction.Normalize();

            for (int i = 0; i < distance; i++)
            {
                Vector2 point = startPoint + direction * i;
                DrawPixelOnTexture(point, lineWidth, lineColor);
            }
        }

        private void DrawPixelOnTexture(Vector2 point, float size, Color color)
        {
            int halfSize = Mathf.CeilToInt(size / 2f);
            for (int x = -halfSize; x <= halfSize; x++)
            for (int y = -halfSize; y <= halfSize; y++)
            {
                int pixelX = Mathf.RoundToInt(point.x + x);
                int pixelY = Mathf.RoundToInt(point.y + y);

                if (pixelX >= 0 && pixelX < texture.width && pixelY >= 0 && pixelY < texture.height)
                    texture.SetPixel(pixelX, pixelY, color);
            }
        }

        public void Apply()
        {
            texture.Apply();
        }
    }
}
