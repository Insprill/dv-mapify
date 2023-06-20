using DV.WeatherSystem;
using Mapify.Editor;
using Mapify.SceneInitializers.Vanilla.GameContent;
using UnityEngine;

namespace Mapify.SceneInitializers.GameContent
{
    [SceneSetupPriority(int.MinValue)]
    public class GameSceneSetup : SceneSetup
    {
        public override void Run()
        {
            foreach (VanillaAsset vanillaAsset in AssetCopier.InstantiatableAssets)
            {
                if ((int)vanillaAsset <= GameContentCopier.START_IDX)
                    continue;
                GameObject gameObject = AssetCopier.Instantiate(vanillaAsset, false, false);
                if (gameObject.name.Contains("Weather"))
                    gameObject.AddComponent<WeatherGUITogglerDV>().guiScript = gameObject.GetComponentInChildren<WeatherEditorGUI>();
                // The [ShopLogic] object must be enabled *after* shops are added.
                if (!gameObject.name.Contains("Shop"))
                    gameObject.SetActive(true);
            }

            // I'm not sure where vanilla creates this because it doesn't have auto create enabled, nor is it in any of the four main scenes, or created in code.
            new GameObject("[YardTracksOrganizer]").AddComponent<YardTracksOrganizer>();
        }
    }
}
