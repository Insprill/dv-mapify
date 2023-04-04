using System.Collections.Generic;
using System.Linq;
using Mapify.Editor.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mapify.Editor.Validators
{
    public class TerrainSceneValidator : SceneValidator
    {
        protected override IEnumerator<Result> ValidateScene(Scene terrainScene, Scene railwayScene, Scene gameContentScene)
        {
            GameObject[] roots = terrainScene.GetRootGameObjects();

            #region Distant Terrain

            int distantTerrainCount = roots.Count(go => go.name == "[distant terrain]");
            if (distantTerrainCount != 1)
                yield return Result.Error($"There must be exactly one [distant terrain] object in the {GetPrettySceneName()} scene! Found {distantTerrainCount}");

            #endregion

            # region Terrain

            Terrain[] terrains = roots
                .SelectMany(go => go.GetComponentsInChildren<Terrain>())
                .ToArray();

            if (terrains.Length == 0)
            {
                yield return Result.Error("There must be at least one terrain object");
                yield break;
            }

            GameObject[] allChildObjects = roots.SelectMany(go => go.GetComponentsInChildren<Transform>())
                .Select(t => t.gameObject)
                .ToArray();

            GameObject[] objectsWithoutTerrain = allChildObjects.Except(terrains.Select(terrain => terrain.gameObject)).ToArray();
            foreach (GameObject go in objectsWithoutTerrain)
            {
                if (go.name == "[distant terrain]") continue;
                Component[] components = go.GetComponents<Component>().Where(comp => comp.GetType() != typeof(Transform)).ToArray();
                switch (components.Length)
                {
                    case 0:
                    case 1 when components[0].GetType().Name == "TerrainGroup":
                        continue;
                    default:
                        yield return Result.Error($"Found invalid object {go.name} in the {GetPrettySceneName()} scene! It should only contain the [distant terrain] object and Terrain", go);
                        break;
                }
            }

            Material m = terrains[0].materialTemplate;
            foreach (Terrain terrain in terrains)
            {
                if (m == terrain.materialTemplate) continue;
                yield return Result.Error("All terrains must use the same material", terrain);
                break;
            }

            MapInfo mapInfo = EditorAssets.FindAsset<MapInfo>();
            if (mapInfo != null)
            {
                mapInfo.terrainMaterial = m;
                mapInfo.terrainCount = terrains.Length;
                mapInfo.worldSize = terrains.CalculateWorldSize();
                mapInfo.terrainPixelError = terrains[0].heightmapPixelError;
                mapInfo.terrainBasemapDistance = terrains[0].basemapDistance;
                mapInfo.terrainDrawInstanced = terrains[0].drawInstanced;
            }

            foreach (Terrain terrain in terrains)
                terrain.tag = "EditorOnly"; // Terrain data is saved separately and rebuilt at runtime

            #endregion
        }

        public override string GetScenePath()
        {
            return "Assets/Scenes/Terrain.unity";
        }
    }
}
