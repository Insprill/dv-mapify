using DV.Utils;
using DV.WorldTools;
using UnityEngine;

namespace Mapify.SceneInitializers.Terrain
{
    public class DistantTerrainSetup : SceneSetup
    {
        public override void Run()
        {
            GameObject distantTerrainObject = new GameObject("[distant terrain]");
            distantTerrainObject.transform.SetParent(WorldMover.OriginShiftParent);

            LevelInfo levelInfo = SingletonBehaviour<LevelInfo>.Instance;
            DistantTerrain distantTerrain = distantTerrainObject.gameObject.AddComponent<DistantTerrain>();
            distantTerrain.trackingReference = SingletonBehaviour<WorldMover>.Instance.playerTracker.GetTrackerTransform();
            distantTerrain.singleTerrainSize = levelInfo.terrainSize;
            distantTerrain.worldScale = levelInfo.worldSize.x;
            distantTerrain.step = 128; // No idea what this means but this is what it's set to in the game.
        }
    }
}
