using Mapify.Editor;
using Mapify.Utils;
using UnityEngine;

namespace Mapify.SceneInitializers.GameContent
{
    [SceneSetupPriority(-10)]
    public class VanillaAssetSetup : SceneSetup
    {
        public override void Run()
        {
            foreach (VanillaObject vanillaObject in Object.FindObjectsOfType<VanillaObject>())
            {
                if (!vanillaObject.BelongsInGameContent()) continue;

                vanillaObject.Replace();
            }
        }
    }
}
