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
                vanillaObject.asset = sw.DivergingTrack.GetComponent<BezierCurve>().Last().localPosition.x < 0
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
