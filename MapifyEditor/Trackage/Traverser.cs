using System;
using Mapify.Editor.Utils;
using UnityEngine;

namespace Mapify.Editor
{
    public class Traverser : Turntable
    {
        public Transform startPoint;
        public Transform endPoint;
        [NonSerialized]
        public Vector3 currentPosition;
        [NonSerialized]
        public float targetPosition;
        public Vector3 min => startPoint.position;
        public Vector3 max => endPoint.position;

        public Vector3 GetPosition(float t)
        {
            return Vector3.Lerp(min, max, t);
        }

        public float CurrentPos()
        {
            return Vectors.InverseLerp(min, max, currentPosition);
        }
    }
}
