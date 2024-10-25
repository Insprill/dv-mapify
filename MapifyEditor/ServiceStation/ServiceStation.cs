using System;
using System.Collections;
using Mapify.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace Mapify.Editor
{
    public class ServiceStation : VisualizableMonoBehaviour
    {
        private const float REFILL_MACHINE_Z_OFFSET = -0.34f;
        private const float DIESEL_FUEL_HOSE_Z_OFFSET = -0.905f;
        internal const string MANUAL_SERVICE_INDICATOR_NAME = "Manual Service Indicator";

        public GameObject dieselFuelStation;

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
            VanillaObject vanillaObject = Array.Find(GetComponentsInChildren<VanillaObject>(), vo => vo.asset == VanillaAsset.ServiceStationMarkerOpen || vo.asset == VanillaAsset.ServiceStationMarkerClosed);
            if (vanillaObject == null)
            {
                Debug.LogError($"Failed to find VanillaObject with a {VanillaAsset.ServiceStationMarkerOpen} or {VanillaAsset.ServiceStationMarkerClosed}!", this);
                yield break;
            }

            vanillaObject.asset = markerType.ToVanillaAsset();
        }

        public override void PositionThing(Transform reference, Transform toMove, int count)
        {
#if UNITY_EDITOR
            if (resources[count] == ServiceResource.Diesel && dieselFuelStation != null)
            {
                GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(dieselFuelStation, toMove);
                go.transform.localPosition = new Vector3(0, 0, DIESEL_FUEL_HOSE_Z_OFFSET);
            }
#endif
            toMove.SetParent(reference.parent);
            toMove.SetPositionAndRotation(reference.position, reference.rotation);
            Vector3 refLocPos = reference.localPosition;
            toMove.localPosition = new Vector3(refLocPos.x, refLocPos.y, refLocPos.z + REFILL_MACHINE_Z_OFFSET * (count + 1));
        }
    }
}
