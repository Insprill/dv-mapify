using System.Collections;
using System.Linq;
using Mapify.Editor.Utils;
using UnityEngine;

namespace Mapify.Editor
{
    public class ServiceStation : VisualizableMonoBehaviour
    {
        private const float REFILL_MACHINE_OFFSET = -0.34f;
        internal const string MANUAL_SERVICE_INDICATOR_NAME = "Manual Service Indicator";

        [Header("Service Station")]
        [Tooltip("What all resources are available at this service station")]
        public ServiceResource[] resources;
        [Tooltip("Which service marker should be used. Open has metal grates in the center, closed is solid concrete")]
        public ServiceMarkerType markerType;

        public Transform ManualServiceIndicator => transform.FindChildByName(MANUAL_SERVICE_INDICATOR_NAME);

        private void OnValidate()
        {
            if (!isActiveAndEnabled)
                return;

            if (ManualServiceIndicator == null)
            {
                Debug.LogError($"Failed to find child {MANUAL_SERVICE_INDICATOR_NAME}", this);
                return;
            }

            UpdateVisuals(resources, ManualServiceIndicator);
            StartCoroutine(UpdateServiceStationMarker());
        }

        private IEnumerator UpdateServiceStationMarker()
        {
            yield return null;
            VanillaObject vanillaObject = GetComponentsInChildren<VanillaObject>().FirstOrDefault(vo => vo.asset == VanillaAsset.ServiceStationMarkerOpen || vo.asset == VanillaAsset.ServiceStationMarkerClosed);
            if (vanillaObject == null)
            {
                Debug.LogError($"Failed to find VanillaObject with a {VanillaAsset.ServiceStationMarkerOpen} or {VanillaAsset.ServiceStationMarkerClosed}!", this);
                yield break;
            }

            vanillaObject.asset = markerType.ToVanillaAsset();
        }

        public override void PositionThing(Transform reference, Transform toMove, int count)
        {
            toMove.SetParent(reference.parent);
            toMove.SetPositionAndRotation(reference.position, reference.rotation);
            Vector3 refLocPos = reference.localPosition;
            toMove.localPosition = new Vector3(refLocPos.x, refLocPos.y, refLocPos.z + REFILL_MACHINE_OFFSET * (count + 1));
        }
    }
}
