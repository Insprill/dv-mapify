#if UNITY_EDITOR
using System.Collections.Generic;
using Mapify.Editor;
using Mapify.Editor.Validators;
using UnityEditor;

namespace MapifyEditor.Export.Validators.Project
{
    public class UnityBuildTargetValidator : Validator
    {
        protected override IEnumerator<Result> Validate(Scenes scenes)
        {
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64))
                yield return Result.Error("Missing build target Standalone Windows x64. Please ensure you have the 'Windows Build Support (Mono)' module installed.");
        }
    }
}
#endif
