using UnityEngine;

namespace Mapify.Components
{
    /// <summary>
    /// Force the target Gameobject to stay active
    /// </summary>
    public class Reactivate: MonoBehaviour
    {
        public GameObject target;

        private void Update()
        {
            if(target.activeSelf) return;

            target.SetActive(true);
            Destroy(this);
        }
    }
}
