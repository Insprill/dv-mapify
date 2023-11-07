using Mapify.Editor;
using Mapify.Editor.Utils;
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
                GameObject go = vanillaObject.Replace(keepChildren: vanillaObject.keepChildren, rotationOffset: vanillaObject.rotationOffset);

                switch (vanillaObject.asset)
                {
                    case VanillaAsset.StationOffice1:
                    case VanillaAsset.StationOffice2:
                    case VanillaAsset.StationOffice3:
                    case VanillaAsset.StationOffice4:
                    case VanillaAsset.StationOffice5:
                    case VanillaAsset.StationOffice6:
                    case VanillaAsset.StationOffice7:
                        // todo: make this show in the correct location instead of removing it. Then we can also get rid of this switch.
                        Transform youAreHereFlag = go.transform.FindChildByName("PinRed");
                        if (youAreHereFlag != null)
                            Object.Destroy(youAreHereFlag.gameObject);
                        break;
                }
            }
        }
    }
}
