using System.Collections.Generic;
using System.Linq;
using DV.CashRegister;
using Mapify.Editor;
using Mapify.Editor.Utils;
using Mapify.Utils;
using UnityEngine;

namespace Mapify.SceneInitializers.GameContent
{
    public class PitstopSetup : SceneSetup
    {
        public override void Run()
        {
            ServiceStation[] serviceStations = Object.FindObjectsOfType<ServiceStation>();
            foreach (ServiceStation serviceStation in serviceStations)
            {
                GameObject pitStopStationObject = AssetCopier.Instantiate(VanillaAsset.PitStopStation, false);
                Transform serviceStationTransform = serviceStation.transform;
                pitStopStationObject.transform.SetPositionAndRotation(serviceStationTransform.position, serviceStationTransform.rotation);

                GameObject manualServiceIndicator = pitStopStationObject.FindChildByName("ManualServiceIndicator");
                Transform manualServiceIndicatorTransform = manualServiceIndicator.transform;

                Transform msi = serviceStation.ManualServiceIndicator;
                manualServiceIndicatorTransform.SetPositionAndRotation(msi.position, msi.rotation);
                Object.Destroy(msi.gameObject);

                //todo: customizable price-per-unit
                List<LocoResourceModule> resourceModules = new List<LocoResourceModule>(serviceStation.resources.Length);
                for (int i = 0; i < serviceStation.resources.Length; i++)
                {
                    ServiceResource resource = serviceStation.resources[i];
                    GameObject moduleObj = AssetCopier.Instantiate((VanillaAsset)resource);
                    serviceStation.PositionThing(manualServiceIndicatorTransform, moduleObj.transform, i);
                    LocoResourceModule resourceModule = moduleObj.GetComponentInChildren<LocoResourceModule>();
                    resourceModules.Add(resourceModule);
                }

                PitStopIndicators pitStopIndicators = pitStopStationObject.GetComponentInChildren<PitStopIndicators>();
                pitStopIndicators.resourceModules = resourceModules.ToArray();

                PitStop pitStop = pitStopStationObject.GetComponentInChildren<PitStop>();

                VanillaObject[] vanillaObjects = serviceStation.GetComponentsInChildren<VanillaObject>();
                foreach (VanillaObject vanillaObject in vanillaObjects)
                {
                    VanillaAsset asset = vanillaObject.asset;
                    if (asset == serviceStation.markerType.ToVanillaAsset())
                    {
                        BoxCollider vCollider = vanillaObject.GetComponent<BoxCollider>();
                        BoxCollider collider = pitStop.gameObject.AddComponent<BoxCollider>();
                        collider.center = vCollider.center;
                        collider.size = vCollider.size;
                        collider.isTrigger = true;
                        vanillaObject.Replace();
                    }
                    else if (asset == VanillaAsset.CashRegister)
                    {
                        GameObject cashRegisterObj = vanillaObject.Replace();
                        CashRegisterWithModules cashRegister = cashRegisterObj.GetComponentInChildren<CashRegisterWithModules>();
                        cashRegister.registerModules = resourceModules.Cast<CashRegisterModule>().ToArray();
                    }
                }

                serviceStation.gameObject.Replace(pitStopStationObject).SetActive(true);
            }
        }
    }
}
