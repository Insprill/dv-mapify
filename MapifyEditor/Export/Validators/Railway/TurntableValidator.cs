#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Mapify.Editor;
using Mapify.Editor.Utils;
using Mapify.Editor.Validators;
using UnityEngine;

namespace MapifyEditor.Export.Validators
{
    public class TurntableValidator : Validator
    {
        protected override IEnumerator<Result> Validate(Scenes scenes)
        {
            foreach (Turntable turntable in scenes.railwayScene.GetAllComponents<Turntable>())
            {
                if (turntable.bridge == null)
                {
                    yield return Result.Error("Turntables must have a bridge set", turntable);
                    continue;
                }

                if (turntable.frontHandle != null)
                {
                    if (!turntable.frontHandle.transform.IsChildOf(turntable.bridge))
                        yield return Result.Error("The front turntable handle must be a child of the bridge", turntable);
                    if (turntable.frontHandle.center != Vector3.zero)
                        yield return Result.Error("The front turntable handle must have it's center at 0, 0, 0", turntable);
                }

                if (turntable.rearHandle != null)
                {
                    if (!turntable.rearHandle.transform.IsChildOf(turntable.bridge))
                        yield return Result.Error("The rear turntable handle must be a child of the bridge", turntable);
                    if (turntable.rearHandle.center != Vector3.zero)
                        yield return Result.Error("The rear turntable handle must have it's center at 0, 0, 0", turntable);
                }

                VanillaObject[] vanillaObjects = turntable.GetComponentsInChildren<VanillaObject>();
                if (vanillaObjects.Count(vo => vo.asset == VanillaAsset.TurntableControlPanel) != 1)
                    yield return Result.Error("Turntables must have exactly one turntable control panel", turntable);

                if (turntable.Track == null)
                {
                    yield return Result.Error("Turntables must have the track set", turntable);
                    continue;
                }

                for (int i = 0; i < turntable.Track.Curve.pointCount; i++)
                {
                    BezierPoint point = turntable.Track.Curve[i];
                    if (point.localPosition.y != 0.0f)
                        yield return Result.Error("Turntable track points must have a local height of 0", point.transform);
                }
            }
        }
    }
}
#endif
