#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Mapify.Editor.Utils;
using UnityEngine;

namespace Mapify.Editor.Validators
{
    public class ServiceStationValidator : Validator
    {
        protected override IEnumerator<Result> Validate(Scenes scenes)
        {
            ServiceStation[] serviceStations = scenes.gameContentScene.GetAllComponents<ServiceStation>();
            if (serviceStations.Length == 0)
                yield return Result.Warning("No Service Stations found! Player's won't be able to manually repair equipment");

            foreach (ServiceStation serviceStation in serviceStations)
            {
                if (serviceStation.resources.Length == 0)
                    yield return Result.Error("Service stations must have at least one resource", serviceStation);
                else if (serviceStation.resources.Contains(ServiceResource.Diesel) && serviceStation.resources[serviceStation.resources.Length - 1] != ServiceResource.Diesel)
                    yield return Result.Error("Service stations with Diesel resource must have it as the last in the list!", serviceStation);

                var duplicateResources = serviceStation.resources.GroupBy(e => e)
                    .Where(g => g.Count() > 1)
                    .Select(g => new { Resource = g.Key, Count = g.Count() })
                    .Distinct();
                foreach (var duplicateResource in duplicateResources)
                    yield return Result.Error($"Service stations can only have one of each resource! Found {duplicateResource.Count} {duplicateResource.Resource}'s", serviceStation);

                if (serviceStation.ManualServiceIndicator == null)
                    yield return Result.Error($"Service stations need exactly one '{ServiceStation.MANUAL_SERVICE_INDICATOR_NAME}'", serviceStation);

                VanillaObject[] vanillaObjects = serviceStation.GetComponentsInChildren<VanillaObject>();
                if (Array.TrueForAll(vanillaObjects, vo => vo.asset != serviceStation.markerType.ToVanillaAsset()))
                    yield return Result.Error("Service stations must have a service station marker", serviceStation);
                else if (Array.Find(vanillaObjects, vo => vo.asset == serviceStation.markerType.ToVanillaAsset()).GetComponent<BoxCollider>() == null)
                    yield return Result.Error("Service station markers must have a BoxCollider", serviceStation);

                if (Array.TrueForAll(vanillaObjects, vo => vo.asset != VanillaAsset.CashRegister))
                    yield return Result.Error("Service stations must have a cash register", serviceStation);
            }
        }
    }
}
#endif
