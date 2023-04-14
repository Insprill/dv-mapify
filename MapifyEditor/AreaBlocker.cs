using UnityEngine;

namespace Mapify.Editor
{
    [RequireComponent(typeof(Collider))]
    public class AreaBlocker : MonoBehaviour
    {
        [Tooltip("The license required to remove this blocker")]
        public JobLicense requiredLicense;
    }
}
