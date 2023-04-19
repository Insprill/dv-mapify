using Mapify.Editor.Utils;

namespace Mapify.Editor.StateUpdaters
{
    public class SwitchUpdater : BuildUpdater
    {
        protected override void Update(Scenes scenes)
        {
            foreach (Switch sw in scenes.gameContentScene.GetAllComponents<Switch>())
            {
                VanillaObject vanillaObject = sw.GetComponent<VanillaObject>();
                vanillaObject.asset = sw.DivergingTrack.GetComponent<BezierCurve>().Last().localPosition.x < 0
                    ? sw.standSide == Switch.StandSide.DIVERGING
                        ? VanillaAsset.SwitchLeftOuterSign
                        : VanillaAsset.SwitchLeft
                    : sw.standSide == Switch.StandSide.DIVERGING
                        ? VanillaAsset.SwitchRightOuterSign
                        : VanillaAsset.SwitchRight;
            }
        }
    }
}
