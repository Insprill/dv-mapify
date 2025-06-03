using UnityEngine;

namespace Mapify.Editor
{
    public class Store : VisualizableMonoBehaviour
    {
        private const float STORE_MODULE_Y_OFFSET = -0.3f;
        private const float STORE_MODULE_X_OFFSET = 0.6f;

        [Header("Store")]

        [Tooltip("Manually specify what items are sold at this shop. If you leave this at False, the shop will have all the base game items.")]
        public bool SpecifyItems = false;

        [Tooltip("What items are sold at this shop. Only used if "+nameof(SpecifyItems)+" is true.")]
        public ItemType[] itemTypes;

        [Header("Custom")]
        [Tooltip("Whether this is a custom shop, or a vanilla one.")]
        public bool isCustom = false;
        [Tooltip("The reference point for generating item modules")]
        public Transform moduleReference;
        [Tooltip("Whether item modules should be generated on the -X axis instead of X")]
        public bool flipModuleDirection;
        [Tooltip("How many item module columns there should be per-row")]
        [Min(1)]
        public int columns;
        [Tooltip("The cash register for this shop. Used as a reference point for placing other assets")]
        public Transform cashRegister;
        [Tooltip("Where purchased items will spawn")]
        public Transform itemSpawnReference;

        public void OnValidate()
        {
            if (!isCustom)
                return;
            if (moduleReference == null)
            {
                Debug.LogError($"You must specify {nameof(moduleReference)} for custom shops!");
                return;
            }

            UpdateVisuals(itemTypes, moduleReference);
        }

        public override void PositionThing(Transform reference, Transform toMove, int count)
        {
            toMove.SetParent(reference.parent, false);

            Vector3 pos = reference.localPosition;

            float f = count / (float)columns;
            int row = Mathf.FloorToInt(f);
            bool isRowStart = f - row < 0.0001;

            float startingZ = pos.z;
            pos.y += STORE_MODULE_Y_OFFSET * row;

            if (isRowStart)
                pos.z = startingZ;
            else if (count != 0)
                pos.z += (flipModuleDirection ? -STORE_MODULE_X_OFFSET : STORE_MODULE_X_OFFSET) * (count - row * columns);

            toMove.localPosition = Application.isEditor ? pos : new Vector3(pos.z, pos.y, pos.x);
        }
    }
}
