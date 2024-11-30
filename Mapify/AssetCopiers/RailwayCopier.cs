using System.Collections.Generic;
using DV;
using Mapify.Editor;
using Mapify.Editor.Utils;
using Mapify.Utils;
using UnityEngine;

namespace Mapify.SceneInitializers.Vanilla.Railway
{
    public class RailwayCopier : AssetCopier
    {
        protected override IEnumerator<(VanillaAsset, GameObject)> ToSave(GameObject gameObject)
        {
            if (gameObject.name != WorldData.RAILWAY_ROOT)
                yield break;

            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                GameObject child = gameObject.transform.GetChild(i).gameObject;
                string name = child.name;
                switch (name)
                {
                    case "Turntable Track":
                        GameObject bridge = child.FindChildByName("bridge");
                        Transform visual = bridge.FindChildByName("visual").transform;
                        visual.localPosition = visual.localPosition.AddY(2.5f);
                        Transform colliders = bridge.FindChildByName("colliders").transform;
                        colliders.localPosition = colliders.localPosition.AddY(2.5f);
                        yield return (VanillaAsset.TurntableTrack, child);
                        yield return (VanillaAsset.TurntableBridge, bridge);
                        continue;
                    case "TurntableControlPanel":
                        yield return (VanillaAsset.TurntableControlPanel, child);
                        // The control panel loses this reference for whatever reason, so we have to copy it too.
                        yield return (VanillaAsset.TurntableRotateLayered, child.GetComponent<TurntableController>().turntableRotateLayered.gameObject);
                        continue;
                    case "term-buffer":
                        yield return (VanillaAsset.BufferStopModel, child.transform.FindChildByName("model").gameObject);
                        continue;
                }

                if (child.transform.rotation.x != 0.0f || child.transform.rotation.z != 0.0f)
                    continue;
                switch (name)
                {
                    case "junc-left":
                        yield return (VanillaAsset.SwitchLeft, child);
                        continue;
                    case "junc-right":
                        yield return (VanillaAsset.SwitchRight, child);
                        continue;
                    case "junc-left-outer-sign":
                        yield return (VanillaAsset.SwitchLeftOuterSign, child);
                        continue;
                    case "junc-right-outer-sign":
                        yield return (VanillaAsset.SwitchRightOuterSign, child);
                        continue;
                }
            }
        }
    }
}
