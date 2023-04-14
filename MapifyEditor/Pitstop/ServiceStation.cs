using System.Collections;
using System.Linq;
using Mapify.Editor.Utils;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEngine;

namespace Mapify.Editor
{
    public class ServiceStation : MonoBehaviour
    {
        private const float REFILL_MACHINE_OFFSET = -0.34f;
        internal const string MANUAL_SERVICE_INDICATOR_NAME = "Manual Service Indicator";

        [Tooltip("Which service marker should be used. Open has metal grates in the center, closed is solid concrete")]
        public ServiceMarkerType markerType;
        [Tooltip("What all resources are available at this service station")]
        public ServiceResource[] resources;

        [Header("Editor Visualization")]
#pragma warning disable CS0649
        [SerializeField]
        private GameObject refillMachinePrefab;
#pragma warning restore CS0649

        public Transform ManualServiceIndicator => transform.FindChildByName(MANUAL_SERVICE_INDICATOR_NAME);

        private void OnValidate()
        {
            if (PrefabStageUtility.GetCurrentPrefabStage() != null || EditorUtility.IsPersistent(gameObject) || refillMachinePrefab == null)
                return;
            StartCoroutine(UpdateVisualRefillMachines());
            UpdateServiceStationMarker();
        }

        private IEnumerator UpdateVisualRefillMachines()
        {
            yield return null;
            DestroyRefillMachines();

            Transform reference = ManualServiceIndicator;
            if (reference == null)
            {
                Debug.LogError($"Failed to find child {MANUAL_SERVICE_INDICATOR_NAME}", this);
                yield break;
            }

            for (int i = 0; i < resources.Length; i++)
            {
                GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(refillMachinePrefab);
                PositionRefillMachine(reference, go.transform, i);
                go.tag = "EditorOnly";
            }
        }

        private void DestroyRefillMachines()
        {
            foreach (Transform child in transform.GetChildren())
                if (child.name == refillMachinePrefab.name)
                    DestroyImmediate(child.gameObject);
        }

        private void UpdateServiceStationMarker()
        {
            VanillaObject vanillaObject = GetComponentsInChildren<VanillaObject>().First(vo => vo.asset == VanillaAsset.ServiceStationMarkerOpen || vo.asset == VanillaAsset.ServiceStationMarkerClosed);
            if (vanillaObject == null)
            {
                Debug.LogError($"Failed to find VanillaObject with a {VanillaAsset.ServiceStationMarkerOpen} or {VanillaAsset.ServiceStationMarkerClosed}!", this);
                return;
            }

            vanillaObject.asset = markerType.ToVanillaAsset();
        }

        public void PositionRefillMachine(Transform reference, Transform toMove, int count)
        {
            toMove.SetParent(reference.parent);
            toMove.SetPositionAndRotation(reference.position, reference.rotation);
            Vector3 refLocPos = reference.localPosition;
            toMove.localPosition = new Vector3(refLocPos.x, refLocPos.y, refLocPos.z + REFILL_MACHINE_OFFSET * (count + 1));
        }
    }
}
