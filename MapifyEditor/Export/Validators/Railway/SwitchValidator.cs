using System.Collections.Generic;
using Mapify.Editor;
using Mapify.Editor.Utils;
using Mapify.Editor.Validators;

namespace MapifyEditor.Export.Validators
{
    public class SwitchValidator : Validator
    {
        protected override IEnumerator<Result> Validate(Scenes scenes)
        {
            foreach (Switch sw in scenes.railwayScene.GetAllComponents<Switch>())
            {
                Track divergingTrack = sw.DivergingTrack.GetComponent<Track>();
                Track throughTrack = sw.ThroughTrack.GetComponent<Track>();
                if (!divergingTrack.isInSnapped || !divergingTrack.isOutSnapped || !throughTrack.isInSnapped || !throughTrack.isOutSnapped)
                    yield return Result.Error("Switches must have a track attached to all points", sw);
            }
        }
    }
}
