#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Mapify.Editor;
using Mapify.Editor.Utils;
using Mapify.Editor.Validators;
using UnityEngine;

namespace MapifyEditor.Export.Validators
{
    public class TerrainValidator : Validator
    {
        protected override IEnumerator<Result> Validate(Scenes scenes)
        {
            Terrain[] terrains = scenes.terrainScene.GetAllComponents<Terrain>().Sort();

            if (terrains.Length == 0)
            {
                yield return Result.Error("There must be at least one terrain object in the terrain scene");
                yield break;
            }

            if (terrains[0].transform.position.x != 0 || terrains[0].transform.position.z != 0)
                yield return Result.Error("Terrain must start at 0, 0 expanding on the positive X and Z axis'", terrains[0]);
            if (terrains[0].transform.position.y < 0)
                yield return Result.Error("Terrain must be above Y 0", terrains[0]);

            bool anyFailed = false;
            foreach (Terrain terrain in terrains)
                if (terrain.terrainData == null)
                {
                    yield return Result.Error("Terrains must have a Terrain Data set", terrain);
                    anyFailed = true;
                }

            if (anyFailed)
                yield break;

            Terrain[] duplicateTerrains = terrains.GroupBy(t => t.terrainData)
                .Where(g => g.Count() > 1)
                .SelectMany(g => g)
                .ToArray();
            foreach (Terrain terrain in duplicateTerrains)
                yield return Result.Error($"Terrain '{terrain.name}' shares TerrainData '{terrain.terrainData.name}' with another Terrain!", terrain);

            Material m = terrains[0].materialTemplate;
            float xSize = terrains[0].terrainData.size.x;
            float ySize = terrains[0].terrainData.size.y;
            foreach (Terrain terrain in terrains)
            {
                if (m != terrain.materialTemplate)
                    yield return Result.Error("All terrains must use the same material", terrain);
                if (Mathf.Abs(xSize - terrain.terrainData.size.x) > 0.001 || Mathf.Abs(xSize - terrain.terrainData.size.z) > 0.001)
                    yield return Result.Error("All terrains must be the same size on the X and Z axis", terrain);
                if (Mathf.Abs(ySize - terrain.terrainData.size.y) > 0.001)
                    yield return Result.Error("All terrains must have the same height", terrain);
            }
        }
    }
}
#endif
