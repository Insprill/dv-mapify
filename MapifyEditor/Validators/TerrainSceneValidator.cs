using System;
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
                Component[] components = go.GetComponents<Component>().Where(comp => comp.GetType() != typeof(Transform)).ToArray();
                switch (components.Length)
                {
                    case 0:
                    case 1 when components[0].GetType().Name == "TerrainGroup":
                        continue;
                    default:
                        yield return Result.Error($"Found invalid object {go.name} in the {GetPrettySceneName()} scene! It should only contain terrain", go);
                        break;
                }
            }

            Material m = terrains[0].materialTemplate;
            float size = terrains[0].terrainData.size.x;
            foreach (Terrain terrain in terrains)
            {
                if (m != terrain.materialTemplate)
                    yield return Result.Error("All terrains must use the same material", terrain);
                if (Math.Abs(size - terrain.terrainData.size.x) > 0.001 || Math.Abs(size - terrain.terrainData.size.z) > 0.001)
                    yield return Result.Error("All terrains must be the same size on the X and Z axis", terrain);
            }

            MapInfo mapInfo = EditorAssets.FindAsset<MapInfo>();
            if (mapInfo != null)
            {
                mapInfo.terrainMaterial = m;
                mapInfo.terrainHeight = terrains[0].transform.position.y;
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
