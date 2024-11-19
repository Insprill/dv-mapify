#if UNITY_EDITOR
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
            foreach (var switch_ in scenes.railwayScene.GetAllComponents<SwitchBase>())
            {
                foreach (var track in switch_.GetComponentsInChildren<Track>())
                {
                    track.Snap();

                    if (track.isInSnapped && track.isOutSnapped) continue;

                    yield return Result.Error("Switches must have a track attached to all points", switch_);
                    break;
                }
            }
        }
    }
}
#endif
