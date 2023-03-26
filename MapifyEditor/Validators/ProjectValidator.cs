using System.Collections.Generic;
using System.Text.RegularExpressions;
using Mapify.Editor.Utils;

namespace Mapify.Editor.Validators
{
    public class ProjectValidator : Validator
    {
        private const string MAP_NAME_REGEX = "[a-zA-Z0-9-_& ]";

        public override IEnumerator<Result> Validate()
        {
            // MapInfo
            MapInfo[] mapInfos = EditorAssets.FindAssets<MapInfo>();
            if (mapInfos.Length != 1) yield return Result.Error($"There should be exactly one MapInfo! Found {mapInfos.Length}");
            if (mapInfos.Length == 1 && !Regex.IsMatch(mapInfos[0].mapName, MAP_NAME_REGEX)) yield return Result.Error($"Your map name must match the following pattern: {MAP_NAME_REGEX}");
        }
    }
}
