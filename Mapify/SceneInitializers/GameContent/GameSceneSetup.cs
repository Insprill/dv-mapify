using System;
using DV.Shops;
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
                if ((int)vanillaAsset < GameContentCopier.START_IDX)
                    continue;
                GameObject gameObject = AssetCopier.Instantiate(vanillaAsset, false, false);
                Mapify.LogDebug(() => $"Instantiated Vanilla Asset {vanillaAsset} ({gameObject.name})");

                WeatherEditorGUI weatherEditorGui = gameObject.GetComponentInChildren<WeatherEditorGUI>();
                if (weatherEditorGui != null)
                {
                    weatherEditorGui.enabled = false;
                    DebugCommands.SetWeatherEditorGUI(weatherEditorGui);
                }

                if (!ShouldEnable(gameObject))
                    continue;

                gameObject.SetActive(true);
            }

            // I'm not sure where vanilla creates this because it doesn't have auto create enabled, nor is it in any of the four main scenes, or created in code.
            new GameObject("[YardTracksOrganizer]").AddComponent<YardTracksOrganizer>();
        }

        private bool ShouldEnable(GameObject gameObject)
        {
            // The GlobalShopController must be enabled after shops are added. It gets enabled in StoreSetup.
            GlobalShopController gsc = gameObject.GetComponentInChildren<GlobalShopController>();
            if (gsc != null)
            {
                gsc.CheckInstance();
                return false;
            }

            // The LogicController must be enabled after stations are setup. It gets enabled in StationSetup.
            LogicController lc = gameObject.GetComponentInChildren<LogicController>();
            if (lc != null)
            {
                lc.CheckInstance();
                return false;
            }

            return true;
        }
    }
}
