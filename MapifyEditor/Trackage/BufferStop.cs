using UnityEngine;

namespace Mapify.Editor
{
    [RequireComponent(typeof(TrackSnappable), typeof(BoxCollider))]
    public class BufferStop : MonoBehaviour
    {
        [Min(0)]
        [Tooltip("The speed (in kph) of the train required for the buffer stop to break")]
        public float breakSpeed = 7f;
        [Min(0)]
        [Tooltip("The mass of the buffer stop's Rigidbody after it breaks")]
        public float massAfterBreak = 30000f;
        [Min(0)]
        [Tooltip("How far, in meters, the buffer stop will compress when hit by a train")]
        public float compressionRange;
        [Tooltip("The point at which the buffer stop will start to compress")]
        public Transform compressionPoint;
        [Tooltip("The collider player's will interact with. Must be on a child of the BufferStop")]
        public BoxCollider playerCollider;
    }
}
