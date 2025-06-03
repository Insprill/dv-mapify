#if UNITY_EDITOR
using System.Collections.Generic;
using Mapify.Editor.Utils;

namespace Mapify.Editor.Validators
{
    public class VanillaAssetValidator : Validator
    {
        protected override IEnumerator<Result> Validate(Scenes scenes)
        {
            foreach (var vanillaObject in scenes.streamingScene.GetAllComponents<VanillaObject>())
            {
                if (!vanillaObject.BelongsInGameContent()) continue;

                yield return Result.Error($"The vanilla asset {vanillaObject} needs to be in the {scenes.gameContentScene.name} scene", vanillaObject);
            }
        }
    }
}
#endif
