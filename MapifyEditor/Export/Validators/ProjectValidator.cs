using System.Collections.Generic;
using System.Text.RegularExpressions;
using Mapify.Editor.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mapify.Editor.Validators
{
    public class ProjectValidator : Validator
    {
        private const string MAP_NAME_REGEX = "[a-zA-Z0-9-_& ]";

        public override IEnumerator<Result> Validate(List<Scene> scenes)
        {
            #region MapInfo

            MapInfo[] mapInfos = EditorAssets.FindAssets<MapInfo>();
            if (mapInfos.Length != 1)
            {
                yield return Result.Error($"There should be exactly one MapInfo! Found {mapInfos.Length}");
                yield break;
            }

            MapInfo mapInfo = mapInfos[0];

            if (!Regex.IsMatch(mapInfo.mapName, MAP_NAME_REGEX))
                yield return Result.Error($"Your map name must match the following pattern: {MAP_NAME_REGEX}", mapInfo);

            if (mapInfo.waterLevel < 0)
                yield return Result.Error("Water level cannot be lower than 0", mapInfo);

            Vector3 spawnPos = mapInfo.defaultSpawnPosition;
            if (spawnPos.x < 0 || spawnPos.z < 0 || spawnPos.x > mapInfo.worldSize || spawnPos.x > mapInfo.worldSize)
                yield return Result.Error($"The spawn position's X and Z values must be within the world's bounds (0-{mapInfo.worldSize}", mapInfo);
            if (spawnPos.y < mapInfo.waterLevel)
                yield return Result.Error($"The spawn position must be above the water level ({mapInfo.waterLevel}", mapInfo);

            #endregion
        }
    }
}
