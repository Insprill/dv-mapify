namespace Mapify.Utils
{
    public static class MathUtils
    {
        /// <summary>
        /// Re-maps a number from one range to another. Source: https://docs.arduino.cc/language-reference/en/functions/math/map/
        /// </summary>
        public static float Map(float value, float in_min, float in_max, float out_min, float out_max) {
            return (value - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
        }
    }
}
