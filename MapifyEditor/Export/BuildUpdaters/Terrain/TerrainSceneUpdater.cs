#if UNITY_EDITOR
using System.Linq;
using Mapify.Editor.Utils;
using UnityEngine;

namespace Mapify.Editor.StateUpdaters
{
    public class TerrainSceneUpdater : BuildUpdater
    {
        protected override void Update(Scenes scenes)
        {
            Terrain[] sortedTerrain = scenes.terrainScene.GetAllComponents<Terrain>()
                .Where(terrain => terrain.gameObject.activeInHierarchy)
                .ToArray()
                .Sort();
            MapRenderer.RenderMap(sortedTerrain);
        }
    }
}
#endif
