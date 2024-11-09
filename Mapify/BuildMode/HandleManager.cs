using System.Collections.Generic;
using UnityEngine;

namespace RuntimeHandle
{
    /// <summary>
    /// Manages the handles the player can use to manipulate objects
    /// </summary>
    public class HandleManager: MonoBehaviour
    {
        private List<RuntimeTransformHandle> handles = new List<RuntimeTransformHandle>();
        private HandleType handleType = HandleType.POSITION;
        private bool active = false;

        public HandleManager(){}

        private void OnDisable()
        {
            foreach (var h in handles)
            {
                Destroy(h);
            }
        }

        public void SetHandleTypes(HandleType aHandleType)
        {
            if(handleType == aHandleType){ return; }
            handleType = aHandleType;

            foreach (var handle in handles)
            {
                handle.SetHandleMode(aHandleType);
            }
        }

        public void Add(Transform targetTransform)
        {
            var handle = RuntimeTransformHandle.Create(targetTransform, handleType);
            handle.transform.SetParent(transform, true);
            handle.gameObject.SetActive(active);
            handles.Add(handle);
        }

        public void SetHandlesActive(bool _active)
        {
            active = _active;

            foreach (var h in handles)
            {
                h.gameObject.SetActive(active);
            }
        }
    }
}
