using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Mapify.Editor.Utils;
using Mapify.Map;
using Mapify.Utils;
using TMPro;
using UnityEngine;

namespace Mapify.Patches
{
    [HarmonyPatch(typeof(WorldMap), "Awake")]
    public static class WorldMap_Awake_Patch
    {
        private static bool modified;

        // todo: Station maps are now the same! Search for the MapPaperOffice in the scene.
        private static void Postfix(WorldMap __instance)
        {
            if (Maps.IsDefaultMap)
                return;
            UpdateStationNames(__instance.transform);
            if (modified) return;
            Material material = __instance.transform.FindChildByName("Map_LOD0").GetComponent<Renderer>().sharedMaterial;
            Texture2D mapTexture = DrawTracksOnMap();
            material.mainTexture = mapTexture;
            modified = true;

            GameObject mapPoster = GameObject.Find("MapPoster");
            if (mapPoster == null)
            {
                Mapify.LogError("Failed to find map poster!");
                return;
            }

            mapPoster.GetComponent<Renderer>().sharedMaterial.mainTexture = mapTexture;
        }

        private static Texture2D DrawTracksOnMap()
        {
            int[] textureSize = Maps.LoadedMap.mapTextureSize;
            Texture2D texture = new Texture2D(textureSize[0], textureSize[1]);
            texture.LoadImage(Maps.LoadedMap.mapTextureSerialized);

            TextureDrawer drawer = new TextureDrawer(texture);

            float worldSize = Maps.LoadedMap.worldSize;

            IEnumerable<Vector2[]> points = RailTrackRegistry.Instance.AllTracks.Select(rt => rt.GetCurvePositions(rt.curve.resolution).ToArray());
            (Vector2, Vector2)[] pairs = points.SelectMany(trackPoints =>
                Enumerable.Range(1, trackPoints.Length - 1)
                    .Select(i => (
                        trackPoints[i - 1].Scale(0, worldSize, 0, textureSize[0]),
                        trackPoints[i].Scale(0, worldSize, 0, textureSize[1]))
                    )
            ).ToArray();

            // The borders must be draw first, otherwise you'll see it dividing each segment of the rail
            if (Maps.LoadedMap.trackBackgroundWidth > 0)
                foreach ((Vector2 startPoint, Vector2 endPoint) in pairs)
                    drawer.DrawLineOnTexture(
                        startPoint + Maps.LoadedMap.trackBackgroundOffset,
                        endPoint + Maps.LoadedMap.trackBackgroundOffset,
                        Maps.LoadedMap.trackWidth + Maps.LoadedMap.trackBackgroundWidth,
                        Maps.LoadedMap.trackBackgroundColor
                    );
            foreach ((Vector2 startPoint, Vector2 endPoint) in pairs)
                drawer.DrawLineOnTexture(startPoint, endPoint, Maps.LoadedMap.trackWidth, Maps.LoadedMap.trackColor);

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
                    .Scale(0, Maps.LoadedMap.worldSize, -0.175f, 0.175f);
                clone.GetComponent<TextMeshPro>().text = controller.stationInfo.Name.ToUpper();
            }

            Object.DestroyImmediate(copy);
        }
    }
}
