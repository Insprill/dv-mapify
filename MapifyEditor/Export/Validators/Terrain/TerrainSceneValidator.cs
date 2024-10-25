#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Mapify.Editor;
using Mapify.Editor.Utils;
using Mapify.Editor.Validators;
using UnityEngine;

namespace MapifyEditor.Export.Validators
{
    public class TerrainSceneValidator : Validator
    {
        protected override IEnumerator<Result> Validate(Scenes scenes)
        {
            Terrain[] terrains = scenes.terrainScene.GetAllComponents<Terrain>();
            GameObject[] terrainGameObjects = terrains.Select(terrain => terrain.gameObject).ToArray();

            GameObject[] allGameObjects = scenes.terrainScene.GetAllGameObjects();
            GameObject[] objectsWithoutTerrain = allGameObjects.Except(terrainGameObjects).ToArray();

            foreach (GameObject go in objectsWithoutTerrain)
            {
                Component[] components = go.GetComponents<Component>().Where(comp => comp.GetType() != typeof(Transform)).ToArray();
                switch (components.Length)
                {
                    case 0:
                    case 1 when components[0].GetType().Name == "TerrainGroup":
                        continue;
                    default:
                        yield return Result.Error($"Found invalid object '{go.name}' in the {scenes.terrainScene.name} scene! It should only contain terrain", go);
                        break;
                }
            }
        }
    }
}
#endif
