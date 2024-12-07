using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Mapify.Editor
{
    public class VanillaLocomotiveSpawner : LocomotiveSpawner
    {
        [SerializeField]
        [Tooltip("What all locomotives to spawn. Each element is a group to spawn together (e.g. the steamer and it's tender)")]
        internal List<VanillaLocomotiveList> locomotiveGroups;

        public override IEnumerable<string> CondenseLocomotiveTypes()
        {
            return locomotiveGroups.Select(types => string.Join(",", types.rollingStock
                .Select(locoType => Enum.GetName(locoType.GetType(), locoType))
            ));
        }
    }
}
