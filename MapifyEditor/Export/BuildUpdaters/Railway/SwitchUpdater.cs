#if UNITY_EDITOR
using Mapify.Editor.Utils;

namespace Mapify.Editor.StateUpdaters
{
    public class SwitchUpdater : BuildUpdater
    {
        protected override void Update(Scenes scenes)
        {
            foreach (VanillaSwitch sw in scenes.railwayScene.GetAllComponents<VanillaSwitch>())
            {
                VanillaObject vanillaObject = sw.GetComponent<VanillaObject>();
                vanillaObject.asset = sw.IsLeft
                    ? sw.standSide == VanillaSwitch.StandSide.DIVERGING
                        ? VanillaAsset.SwitchLeft
                        : VanillaAsset.SwitchLeftOuterSign
                    : sw.standSide == VanillaSwitch.StandSide.DIVERGING
                        ? VanillaAsset.SwitchRight
                        : VanillaAsset.SwitchRightOuterSign;
            }
        }
    }
}
#endif
