using DV.TerrainSystem;
using UnityEngine;

namespace Mapify.SceneInitializers.Terrain
{
    public class TerrainHoleFinderSetup : SceneSetup
    {
        public override void Run()
        {
            TerrainHoleManager terrainHoleManager = new GameObject(nameof(TerrainHoleManager)).AddComponent<TerrainHoleManager>();
            terrainHoleManager.maxHoles = 100; // Todo: update when tunnels are a thing
        }
    }
}
