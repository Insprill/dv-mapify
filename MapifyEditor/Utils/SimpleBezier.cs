using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Mapify.Editor.Utils
{
    public struct SimpleBezier
    {
        public Vector3 P0;
        public Vector3 P1;
        public Vector3 P2;
        public Vector3 P3;

        // Helpers for those who prefer this format.
        public Vector3 Start { get => P0; set => P0 = value; }
        public Vector3 StartHandle { get => P1; set => P1 = value; }
        public Vector3 End { get => P3; set => P3 = value; }
        public Vector3 EndHandle { get => P2; set => P2 = value; }

        public SimpleBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            P0 = p0;
            P1 = p1;
            P2 = p2;
            P3 = p3;
        }

        public Vector3[] Sample(int samples = 8)
        {
            return MathHelper.SampleBezier(this, samples);
        }
    }
}
