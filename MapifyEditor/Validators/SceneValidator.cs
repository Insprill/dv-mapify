using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

namespace Mapify.Editor.Validators
{
    public abstract class SceneValidator : Validator
    {
        public override IEnumerator<Result> Validate(List<Scene> scenes)
        {
            Scene terrainScene = scenes.FirstOrDefault(s => s.path == new TerrainSceneValidator().GetScenePath());
            Scene railwayScene = scenes.FirstOrDefault(s => s.path == new RailwaySceneValidator().GetScenePath());
            Scene gameContentScene = scenes.FirstOrDefault(s => s.path == new GameContentSceneValidator().GetScenePath());
            IEnumerator<Result> validateRailwayScene = ValidateScene(terrainScene, railwayScene, gameContentScene);
            while (validateRailwayScene.MoveNext()) yield return validateRailwayScene.Current;
        }

        protected abstract IEnumerator<Result> ValidateScene(Scene terrainScene, Scene railwayScene, Scene gameContentScene);

        public string GetPrettySceneName()
        {
            return GetScenePath().Split('/').Last().Split('.').First();
        }

        public abstract string GetScenePath();
    }
}
