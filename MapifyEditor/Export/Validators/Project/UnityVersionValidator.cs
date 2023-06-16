#if UNITY_EDITOR
using System.Collections.Generic;
using Mapify.Editor;
using Mapify.Editor.Validators;

namespace MapifyEditor.Export.Validators.Project
{
    public class UnityVersionValidator : Validator
    {
        protected override IEnumerator<Result> Validate(Scenes scenes)
        {
            if (!UnityVersionChecker.IsCorrectVersion())
                yield return Result.Error(UnityVersionChecker.ERROR_MESSAGE);
        }
    }
}
#endif
