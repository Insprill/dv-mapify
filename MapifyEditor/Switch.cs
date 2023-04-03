using UnityEngine;

namespace Mapify.Editor
{
    public class Switch : MonoBehaviour
    {
        public enum StandSide
        {
            THROUGH,
            DIVERGING
        }

        public StandSide standSide;
        [HideInNormalInspector]
        public bool inTrackFirst;
        [HideInNormalInspector]
        public Track inTrack;
        [HideInNormalInspector]
        public Track throughTrack;
        [HideInNormalInspector]
        public Track divergingTrack;
        [HideInNormalInspector]
        public bool isDivergingLeft;
        [HideInInspector]
        public GameObject tracksParent;

        public VanillaAsset SwitchPrefabName => isDivergingLeft
            ? standSide == StandSide.DIVERGING
                ? VanillaAsset.SwitchLeftOuterSign
                : VanillaAsset.SwitchLeft
            : standSide == StandSide.DIVERGING
                ? VanillaAsset.SwitchRightOuterSign
                : VanillaAsset.SwitchRight;

        public static Switch CreateSwitch(Vector3 position, Quaternion rotation, bool left)
        {
            GameObject railwayParent = GameObject.Find("[railway]");
            GameObject switchObject = new GameObject("Switch");
            Switch sw = switchObject.AddComponent<Switch>();
            CreateSwitchTrack(switchObject, sw, "[track through]", false, left);
            CreateSwitchTrack(switchObject, sw, "[track diverging]", true, left);

            Transform t = switchObject.transform;
            t.SetPositionAndRotation(position, rotation);
            t.SetParent(railwayParent.transform);

            return sw;
        }

        private static void CreateSwitchTrack(GameObject parent, Switch sw, string name, bool diverging, bool left)
        {
            GameObject trackObject = new GameObject(name);
            BezierCurve curve = trackObject.AddComponent<BezierCurve>();
            curve.resolution = 0.5f;
            curve.close = false;
            curve.drawColor = diverging ? new Color32(230, 118, 23, 255) : new Color32(0, 149, 255, 255);
            BezierPoint point0 = curve.CreatePointAt(new Vector3(0, 0, -12f));
            curve.AddPoint(point0);

            BezierPoint point1 = curve.CreatePointAt(new Vector3(diverging ? -1.83f : 0, 0, 12f));
            curve.AddPoint(point1);
            if (!diverging)
            {
                point0.handle1 = new Vector3(0, 0, -3);
                point0.handle2 = new Vector3(0, 0, 3);
                point0.localPosition = new Vector3(0, 0, -12);
                point1.handle1 = new Vector3(0, 0, -3);
                point1.handle2 = new Vector3(0, 0, 3);
                point1.localPosition = new Vector3(0, 0, 12);
            }
            else
            {
                point0.handle1 = new Vector3(0, 0, -6);
                point0.handle2 = new Vector3(0, 0, 6);
                point0.localPosition = new Vector3(0, 0, -12);
                float direction = left ? 1.0f : -1.0f;
                point1.handle1 = new Vector3(1.437454f * direction, 0, -10.80206f);
                point1.handle2 = new Vector3(-1.437454f * direction, 0, 10.80206f);
                point1.localPosition = new Vector3(-1.830024f * direction, 0, 12);
            }

            Track track = trackObject.AddComponent<Track>();
            track.inSwitch = sw;
            if (diverging)
                sw.divergingTrack = track;
            else
                sw.throughTrack = track;

            Transform t = trackObject.transform;
            t.parent = parent.transform;
            t.localPosition = new Vector3(0, 0, 12);
            t.localRotation = Quaternion.identity;
        }
    }
}
