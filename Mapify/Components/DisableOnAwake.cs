using UnityEngine;

namespace Mapify.Components
{
    /// <summary>
    ///     Disables the attached <see cref="GameObject" /> when Awake is called.
    /// </summary>
    public class DisableOnAwake : MonoBehaviour
    {
        private void Awake()
        {
            gameObject.SetActive(false);
        }
    }
}
