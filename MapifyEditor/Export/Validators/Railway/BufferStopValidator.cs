using System.Collections.Generic;
using Mapify.Editor;
using Mapify.Editor.Utils;
using Mapify.Editor.Validators;

namespace MapifyEditor.Export.Validators
{
    public class BufferStopValidator : Validator
    {
        protected override IEnumerator<Result> Validate(Scenes scenes)
        {
            foreach (BufferStop bufferStop in scenes.railwayScene.GetAllComponents<BufferStop>())
            {
                if (bufferStop.playerCollider == null)
                {
                    yield return Result.Error("Buffer stops must have a Player Collider set", bufferStop);
                    continue;
                }

                if (bufferStop.playerCollider.gameObject == bufferStop.gameObject || !bufferStop.playerCollider.transform.IsChildOf(bufferStop.transform))
                    yield return Result.Error("A Buffer stop's Player Collider must be on a child GameObject of the BufferStop", bufferStop.playerCollider);
            }
        }
    }
}
