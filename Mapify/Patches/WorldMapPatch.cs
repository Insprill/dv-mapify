using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Mapify.Editor.Utils;
using Mapify.Utils;
using TMPro;
using UnityEngine;

namespace Mapify.Patches
{
    [HarmonyPatch(typeof(WorldMap), "Awake")]
    public static class WorldMap_Awake_Patch
    {
        private const float TRACK_SAMPLE_RESOLUTION = 40f;

        private static bool modified;

        private static void Postfix(WorldMap __instance)
        {
            UpdateStationNames(__instance.transform);
            if (modified) return;
            Material material = __instance.transform.FindChildByName("Map_LOD0").GetComponent<Renderer>().sharedMaterial;
            material.mainTexture = DrawTracksOnMap();
            modified = true;
        }

        private static Texture2D DrawTracksOnMap()
        {
            int[] textureSize = Main.LoadedMap.mapTextureSize;
            Texture2D texture = new Texture2D(textureSize[0], textureSize[1]);
            texture.LoadImage(Main.LoadedMap.mapTextureSerialized);

            TextureDrawer drawer = new TextureDrawer(texture);

            float worldSize = Main.LoadedMap.worldSize;

            IEnumerable<Vector2[]> points = RailTrackRegistry.AllTracks.Select(rt => rt.GetCurvePositions(TRACK_SAMPLE_RESOLUTION).ToArray());
            (Vector2, Vector2)[] pairs = points.SelectMany(trackPoints =>
                Enumerable.Range(1, trackPoints.Length - 1)
                    .Select(i => (
                        trackPoints[i - 1].Scale(0, worldSize, 0, textureSize[0]),
                        trackPoints[i].Scale(0, worldSize, 0, textureSize[1]))
                    )
            ).ToArray();

            // The borders must be draw first, otherwise you'll see it dividing each segment of the rail
            if (Main.LoadedMap.trackBorderWidth > 0)
                foreach ((Vector2 startPoint, Vector2 endPoint) in pairs)
                    drawer.DrawLineOnTexture(startPoint, endPoint, Main.LoadedMap.trackWidth + Main.LoadedMap.trackBorderWidth, Main.LoadedMap.trackBorderColor);
            foreach ((Vector2 startPoint, Vector2 endPoint) in pairs)
                drawer.DrawLineOnTexture(startPoint, endPoint, Main.LoadedMap.trackWidth, Main.LoadedMap.trackColor);

            drawer.Apply();

            return drawer.texture;
        }

        private static void UpdateStationNames(Transform map)
        {
            Transform names = map.FindChildByName("Names");
            GameObject copy = Object.Instantiate(names.GetChild(0).gameObject);

            foreach (Transform child in names.GetChildren())
            {
                if (child.name == "Legend") continue;
                Object.DestroyImmediate(child.gameObject);
            }

            foreach (StationController controller in StationController.allStations)
            {
                GameObject clone = Object.Instantiate(copy, names);
                RectTransform rect = clone.GetComponent<RectTransform>();
                // Truly one of the chained method calls of all time
                rect.localPosition = controller.GetComponent<StationJobGenerationRange>().stationCenterAnchor.position.ToXZ()
                    .Scale(0, Main.LoadedMap.worldSize, -0.175f, 0.175f);
                clone.GetComponent<TextMeshPro>().text = controller.stationInfo.Name.ToUpper();
            }

            Object.DestroyImmediate(copy);
        }
    }
}
