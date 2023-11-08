#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Mapify.Editor;
using Mapify.Editor.Utils;
using Mapify.Editor.Validators;
using UnityEngine;

namespace MapifyEditor.Export.Validators.Project
{
    public class MapInfoValidator : Validator
    {
        private const string MAP_NAME_REGEX = "[a-zA-Z0-9-_& ]";

        protected override IEnumerator<Result> Validate(Scenes scenes)
        {
            MapInfo[] mapInfos = EditorAssets.FindAssets<MapInfo>();
            if (mapInfos.Length != 1)
            {
                yield return Result.Error($"There should be exactly one MapInfo! Found {mapInfos.Length}");
                yield break;
            }

            MapInfo mapInfo = mapInfos[0];

            if (!Regex.IsMatch(mapInfo.name, MAP_NAME_REGEX))
                yield return Result.Error($"Your map name must match the following pattern: {MAP_NAME_REGEX}", mapInfo);
            if (mapInfo.name == Names.DEFAULT_MAP_NAME)
                yield return Result.Error($"Your map name cannot be {Names.DEFAULT_MAP_NAME}");

            if (mapInfo.waterLevel < -1)
                yield return Result.Error("Water level cannot be lower than -1", mapInfo);

            Terrain[] terrains = scenes.terrainScene.GetAllComponents<Terrain>();
            float worldSize = terrains.CalculateWorldSize();
            float worldHeight = terrains[0].transform.position.y;

            Vector3 spawnPos = mapInfo.defaultSpawnPosition;
            if (spawnPos.x < 0 || spawnPos.z < 0 || spawnPos.x > worldSize || spawnPos.x > worldSize)
                yield return Result.Error($"The spawn position's X and Z values must be within the world's bounds (0-{worldSize})", mapInfo);
            if (spawnPos.y < worldHeight)
                yield return Result.Error($"The spawn position's Y value must be above the terrain ({worldHeight})", mapInfo);
            if (spawnPos.y < mapInfo.waterLevel)
                yield return Result.Error($"The spawn position must be above the water level ({mapInfo.waterLevel}", mapInfo);

            if (mapInfo.useFixedMapImage)
            {
                if (mapInfo.fixedMapImage == null)
                {
                    yield return Result.Error($"MapInfo: '{nameof(MapInfo.fixedMapImage)}' must be set when '{nameof(MapInfo.useFixedMapImage)}' is true", mapInfo);
                }
                else if(mapInfo.fixedMapImage.width != mapInfo.fixedMapImage.height)
                {
                    yield return Result.Warning($"MapInfo: '{nameof(MapInfo.fixedMapImage)}' should be square or it will be stretched. Current dimensions: {mapInfo.fixedMapImage.width}x{mapInfo.fixedMapImage.height}", mapInfo);
                }
            }
        }
    }
}
#endif
