#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
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
                var switchTracks = switch_.GetTracks();
                if (switchTracks.Length < 2)
                {
                    yield return Result.Error("Switches must have at least 2 branches", switch_);
                }

                foreach (var track in switchTracks)
                {
                    track.Snap();

                    if (track.isInSnapped && track.isOutSnapped) continue;

                    yield return Result.Error("Switches must have a track attached to all points", switch_);
                    break;
                }

                //TODO valideer dat de tracks met de [0] aan elkaar zitten
            }
        }
    }
}
#endif
