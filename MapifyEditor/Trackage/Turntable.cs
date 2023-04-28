using UnityEngine;

namespace Mapify.Editor
{
    public class Turntable : MonoBehaviour
    {
        public BoxCollider frontHandle;
        public BoxCollider rearHandle;
        public Transform bridge;
        public Track Track => GetComponentInChildren<Track>();
    }
}
