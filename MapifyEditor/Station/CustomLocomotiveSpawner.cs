using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Mapify.Editor
{
    public class CustomLocomotiveSpawner : LocomotiveSpawner
    {
        [SerializeField]
        [Tooltip("What all locomotives to spawn. Each element is a group to spawn together (e.g. the steamer and it's tender)")]
        internal List<CustomRollingStockList> locomotiveGroups;

        public override IEnumerable<string> CondenseLocomotiveTypes()
        {
            return locomotiveGroups.Select(types => string.Join(",", types.rollingStock));
        }
    }
}
