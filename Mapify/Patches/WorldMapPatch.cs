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
        private const float TRACK_WIDTH = 4.0f;
        private static readonly Color TRACK_COLOR = Color.black;

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

            IEnumerable<Vector2[]> points = Object.FindObjectsOfType<RailTrack>().Select(rt => rt.GetCurvePositions(40f).ToArray());
            foreach (Vector2[] trackPoints in points)
                for (int i = 1; i < trackPoints.Length; i++)
                {
                    Vector2 startPoint = trackPoints[i - 1].Scale(0, worldSize, 0, textureSize[0]);
                    Vector2 endPoint = trackPoints[i].Scale(0, worldSize, 0, textureSize[1]);
                    drawer.DrawLineOnTexture(startPoint, endPoint, TRACK_WIDTH, TRACK_COLOR);
                }

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

            foreach (StationController controller in Object.FindObjectsOfType<StationController>())
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
