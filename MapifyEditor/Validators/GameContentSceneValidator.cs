using System.Collections.Generic;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mapify.Editor.Validators
{
    public class GameContentSceneValidator : SceneValidator
    {
        protected override IEnumerator<Result> ValidateScene(Scene scene)
        {
            GameObject[] roots = scene.GetRootGameObjects();

            #region Lights

            Light[] lights = roots.SelectMany(go => go.GetComponentsInChildren<Light>()).ToArray();
            int directionalLightCount = lights.Count(light => light.type == LightType.Directional);
            if (directionalLightCount != 1)
                yield return Result.Error($"There must be exactly one directional light in the {GetPrettySceneName()} scene. Found {directionalLightCount}");

            #endregion

            #region Stations

            string railwayScenePath = new RailwaySceneValidator().GetScenePath();
            Scene railwayScene = EditorSceneManager.GetSceneByPath(railwayScenePath);
            bool isRailwaySceneLoaded = railwayScene.isLoaded;
            if (!isRailwaySceneLoaded)
                EditorSceneManager.OpenScene(railwayScenePath, OpenSceneMode.Additive);
            string[] nonSwitchTrackNames = railwayScene.GetRootGameObjects()
                .SelectMany(go => go.GetComponentsInChildren<Track>())
                .Where(track => track.GetComponentInParent<Switch>() == null)
                .Select(go => go.name)
                .ToArray();
            foreach (Station station in roots.SelectMany(go => go.GetComponentsInChildren<Station>()))
            {
                // Track names
                IEnumerable<string> trackNames = station.storageTrackNames.Concat(station.transferInTrackNames).Concat(station.transferOutTrackNames);
                foreach (string stationStorageTrackName in trackNames)
                {
                    int matchCount = nonSwitchTrackNames.Count(name => name == stationStorageTrackName);
                    if (matchCount == 0)
                        yield return Result.Error($"Failed to find track {stationStorageTrackName}!", station);
                    else if (matchCount > 1)
                        yield return Result.Error($"Found multiple tracks with name {stationStorageTrackName}! Track names should be unique.", station);
                }

                // Teleport location
                if (station.teleportLocation == null)
                    yield return Result.Error($"You must set a teleport location for station {station.displayName}!", station);

                // Job booklet spawn area
                VanillaObject vanillaObject = station.GetComponent<VanillaObject>();
                if ((vanillaObject == null || !$"{vanillaObject.asset}".StartsWith("Station")) && station.bookletSpawnArea == null)
                    yield return Result.Error($"You must specify a job booklet spawn area for custom station {station.displayName}!", station);
            }

            if (!isRailwaySceneLoaded)
                EditorSceneManager.UnloadSceneAsync(railwayScenePath);

            #endregion
        }

        public override string GetScenePath()
        {
            return "Assets/Scenes/GameContent.unity";
        }
    }
}
