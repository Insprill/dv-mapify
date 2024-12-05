#if UNITY_EDITOR
using Mapify.Editor.Utils;

namespace Mapify.Editor.StateUpdaters
{
    public class SwitchUpdater : BuildUpdater
    {
        protected override void Update(Scenes scenes)
        {
            foreach (Switch sw in scenes.railwayScene.GetAllComponents<Switch>())
            {
                VanillaObject vanillaObject = sw.GetComponent<VanillaObject>();
                vanillaObject.asset = sw.IsLeft
                    ? sw.standSide == Switch.StandSide.DIVERGING
                        ? VanillaAsset.SwitchLeft
                        : VanillaAsset.SwitchLeftOuterSign
                    : sw.standSide == Switch.StandSide.DIVERGING
                        ? VanillaAsset.SwitchRight
                        : VanillaAsset.SwitchRightOuterSign;
            }
        }
    }
}
#endif
