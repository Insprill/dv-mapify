using DV.Utils;
using Mapify.Editor;
using UnityEngine;

namespace Mapify.SceneInitializers.GameContent
{
    public class WaterSetup : SceneSetup
    {
        public override void Run()
        {
            GameObject water = AssetCopier.Instantiate(VanillaAsset.Water);
            water.transform.position = new Vector3(0, SingletonBehaviour<LevelInfo>.Instance.waterLevel, 0);
        }
    }
}
