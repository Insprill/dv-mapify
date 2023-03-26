using System.Collections.Generic;
using Mapify.Editor.Validators;

namespace Mapify.Editor
{
    // Me: Can we have CCLs TrainCarValidator?
    // Mom: We have CCLs TrainCarValidator at home
    // TrainCarValidator at home:
    public static class MapValidator
    {
        public static IEnumerator<Result> Validate()
        {
            Validator[] validators = {
                new ProjectValidator(),
                new RailwaySceneValidator(),
                new TerrainSceneValidator(),
                new GameContentSceneValidator()
            };

            foreach (Validator validator in validators)
            {
                IEnumerator<Result> results = validator.Validate();
                while (results.MoveNext()) yield return results.Current;
            }
        }
    }
}
