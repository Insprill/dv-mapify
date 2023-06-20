#if UNITY_EDITOR
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
                if (bufferStop.compressionPoint == null)
                    yield return Result.Error("Buffer stops must have a Detection Point set", bufferStop);
                if (bufferStop.compressionPoint != null && (bufferStop.compressionPoint.gameObject == bufferStop.gameObject || !bufferStop.compressionPoint.transform.IsChildOf(bufferStop.transform)))
                    yield return Result.Error("A Buffer stop's Detection Point must be on a child GameObject of the BufferStop", bufferStop.compressionPoint);

                if (bufferStop.playerCollider == null)
                    yield return Result.Error("Buffer stops must have a Player Collider set", bufferStop);
                if (bufferStop.playerCollider != null && (bufferStop.playerCollider.gameObject == bufferStop.gameObject || !bufferStop.playerCollider.transform.IsChildOf(bufferStop.transform)))
                    yield return Result.Error("A Buffer stop's Player Collider must be on a child GameObject of the BufferStop", bufferStop.playerCollider);
            }
        }
    }
}
#endif
