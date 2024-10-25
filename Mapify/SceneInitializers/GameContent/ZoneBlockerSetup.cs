using DV.ThingTypes;
using DV.ThingTypes.TransitionHelpers;
using Mapify.Editor;
using Mapify.Utils;
using UnityEngine;

namespace Mapify.SceneInitializers.GameContent
{
    public class ZoneBlockerSetup : SceneSetup
    {
        public override void Run()
        {
            foreach (AreaBlocker areaBlocker in Object.FindObjectsOfType<AreaBlocker>())
            {
                GameObject go = areaBlocker.gameObject;
                StationLicenseZoneBlocker zoneBlocker = go.AddComponent<StationLicenseZoneBlocker>();
                zoneBlocker.requiredJobLicense = areaBlocker.requiredLicense.ConvertByName<JobLicense, JobLicenses>().ToV2();
                zoneBlocker.blockerObjectsParent = go;
                InvalidTeleportLocationReaction reaction = go.AddComponent<InvalidTeleportLocationReaction>();
                reaction.blocker = zoneBlocker;
                zoneBlocker.tag = "NO_TELEPORT";
            }
        }
    }
}
