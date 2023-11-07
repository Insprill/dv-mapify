using UnityEngine;
using UnityEngine.Serialization;

namespace Mapify.Editor
{
    public class VanillaObject : MonoBehaviour
    {
        public VanillaAsset asset;
        public bool keepChildren = true;
        [FormerlySerializedAs("rotation")] public Vector3 rotationOffset = Vector3.zero;
    }
}
