using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Mapify.Editor
{
    public class CustomLocomotiveSpawner : LocomotiveSpawner
    {
#pragma warning disable CS0649
        [SerializeField]
        [Tooltip("What all locomotives to spawn. Each element is a group to spawn together (e.g. the steamer and it's tender)")]
        internal List<CustomRollingStockList> locomotiveGroups;
#pragma warning restore CS0649

        public override IEnumerable<string> CondenseLocomotiveTypes()
        {
            return locomotiveGroups.Select(types => string.Join(",", types.rollingStock));
        }
    }
}
