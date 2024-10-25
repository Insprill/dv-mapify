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

            foreach (Terrain terrain in terrains
                         .GroupBy(t => t.terrainData)
                         .Where(g => g.Count() > 1)
                         .SelectMany(g => g))
                yield return Result.Error($"Terrain '{terrain.name}' shares TerrainData '{terrain.terrainData.name}' with another Terrain!", terrain);

            #region Material & Size

            Material m = terrains[0].materialTemplate;
            Vector3 terrainSize = terrains[0].terrainData.size;
            float yPosition = terrains[0].transform.position.y;
            foreach (Terrain terrain in terrains)
            {
                if (m != terrain.materialTemplate)
                    yield return Result.Error("All terrains must use the same material", terrain);
                if (!terrain.terrainData.size.Equals(terrainSize))
                    yield return Result.Error("All terrains must be the same size", terrain);
                if (!Mathf.Approximately(terrain.transform.position.y, yPosition))
                    yield return Result.Error("All terrains must be at the same Y level", terrain);
            }

            #endregion

            # region Forms a square

            int rowCount = (int)Mathf.Sqrt(terrains.Length);
            int columnCount = terrains.Length / rowCount;
            for (int i = 0; i < terrains.Length; i++)
            {
                int row = i / columnCount;
                int column = i % columnCount;
                Vector3 expectedPosition = new Vector3(column * terrainSize.x, yPosition, row * terrainSize.x);
                if (!terrains[i].transform.position.Equals(expectedPosition))
                    yield return Result.Error("All terrains must form a perfect square!", terrains[i]);
            }

            #endregion
        }
    }
}
#endif
