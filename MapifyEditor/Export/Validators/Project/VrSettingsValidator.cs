#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Mapify.Editor;
using Mapify.Editor.Validators;
using UnityEditor;

namespace MapifyEditor.Export.Validators.Project
{
    public class VrSettingsValidator : Validator
    {
        protected override IEnumerator<Result> Validate(Scenes scenes)
        {
            // Obsolete VR settings.
#pragma warning disable 0618
            if (!PlayerSettings.virtualRealitySupported) yield return Result.Error("VR support isn't enabled");
            if (!PlayerSettings.singlePassStereoRendering) yield return Result.Error("VR Stereo Rendering Mode isn't set to Single Pass");
            string[] sdks = PlayerSettings.GetVirtualRealitySDKs(BuildTargetGroup.Standalone);
            if (!sdks.Contains("Oculus")) yield return Result.Error("Oculus support isn't enabled");
            if (!sdks.Contains("OpenVR")) yield return Result.Error("OpenVR support isn't enabled");
#pragma warning restore 0618
        }
    }
}
#endif
