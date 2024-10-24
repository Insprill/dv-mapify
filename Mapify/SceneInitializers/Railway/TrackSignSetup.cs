using DV.Signs;
using Mapify.Utils;

namespace Mapify.SceneInitializers.Railway
{
    [SceneSetupPriority(int.MaxValue)]
    public class TrackSignSetup : SceneSetup
    {
        public override void Run()
        {
            WorldMover.OriginShiftParent.gameObject.NewChild("Signs").AddComponent<SignPlacer>();
        }
    }
}
