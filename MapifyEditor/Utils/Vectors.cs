using UnityEngine;

namespace Mapify.Editor.Utils
{
    public static class Vectors
    {
        public static float InverseLerp(Vector3 a, Vector3 b, Vector3 value)
        {
            Vector3 AB = b - a;
            Vector3 AV = value - a;
            float t = Vector3.Dot(AV, AB) / Vector3.Dot(AB, AB);
            return Mathf.Clamp01(t);
        }

        public static float DistanceAlongLine(Vector3 startPoint, Vector3 endPoint, Vector3 checkPoint)
        {
            Vector3 direction = endPoint - startPoint;
            Vector3 projection = startPoint + Vector3.Dot(direction, checkPoint - startPoint) / direction.sqrMagnitude * direction;
            float distanceAlongLine = Vector3.Distance(startPoint, projection);
            return Mathf.Clamp01(distanceAlongLine / direction.magnitude);
        }

        public static (float distance01, int side) GetDistanceAndSide(Vector3 startPoint, Vector3 endPoint, Vector3 checkPoint)
        {
            Vector3 direction = endPoint - startPoint;
            Vector3 projection = startPoint + Vector3.Dot(direction, checkPoint - startPoint) / direction.sqrMagnitude * direction;
            float distanceAlongLine = Vector3.Distance(startPoint, projection);
            float distance01 = Mathf.Clamp01(distanceAlongLine / direction.magnitude);

            Vector3 startToCheck = checkPoint - startPoint;
            Vector3 cross = Vector3.Cross(direction, startToCheck);
            int side = cross.y > 0 ? 1 : -1;

            return (distance01, side);
        }
    }
}
