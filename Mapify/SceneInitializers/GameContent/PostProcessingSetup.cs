using Mapify.Utils;
using SCPE;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Mapify.SceneInitializers.GameContent
{
    public class PostProcessingSetup : SceneSetup
    {
        /// <seealso cref="GraphicsOptions.GlobalPostProcessVolume" />
        public override void Run()
        {
            GameObject obj = GameObject.Find("[GlobalPostProcessing]") ?? new GameObject("[GlobalPostProcessing]");
            Layer.PostProcessing.Apply(obj);
            PostProcessVolume volume = obj.WithComponent<PostProcessVolume>();
            volume.isGlobal = true;
            PostProcessProfile profile = volume.profile;
            if (!profile.HasSettings<AmbientOcclusion>()) profile.AddSettings<AmbientOcclusion>();
            if (!profile.HasSettings<AutoExposure>()) profile.AddSettings<AutoExposure>();
            if (!profile.HasSettings<Bloom>()) profile.AddSettings<Bloom>();
            if (!profile.HasSettings<ColorGrading>()) profile.AddSettings<ColorGrading>();
            if (!profile.HasSettings<MotionBlur>()) profile.AddSettings<MotionBlur>();
            if (!profile.HasSettings<Sunshafts>()) profile.AddSettings<Sunshafts>();
        }
    }
}
