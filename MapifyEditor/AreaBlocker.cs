using UnityEngine;

namespace Mapify.Editor
{
    [RequireComponent(typeof(Collider))]
    public class AreaBlocker : MonoBehaviour
    {
        public JobLicense requiredLicense;
    }
}
