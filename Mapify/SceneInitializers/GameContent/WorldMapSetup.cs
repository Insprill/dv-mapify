using System.Collections.Generic;
using System.Linq;
using I2.Loc;
using Mapify.Editor;
using Mapify.Editor.Utils;
using Mapify.Map;
using Mapify.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Mapify.SceneInitializers.GameContent
{
    [SceneSetupPriority(int.MaxValue)]
    public class WorldMapSetup : SceneSetup
    {
        private static bool modifiedMaterial;
        private static Texture defaultTexture;
        public static bool showStationNamesOnMap = false;

        public override void Run()
        {
            MapLifeCycle.OnCleanup += () => modifiedMaterial = false;
            Transform originShiftParent = WorldMover.Instance.originShiftParent;
            foreach (Transform transform in originShiftParent.FindChildrenByName("MapPaperOffice"))
                UpdateMap(transform);
            foreach (Transform transform in originShiftParent.FindChildrenByName("MapLocationOverview"))
                UpdateMapOverview(transform);
        }

        private static void UpdateMapOverview(Transform transform)
        {
            GameObject listItemPrefab = transform.gameObject.FindChildByName("MapLocationOverviewListItem");
            if (listItemPrefab == null)
            {
                Mapify.LogError($"Failed to find 'MapLocationOverviewListItem' under '{transform.name}'!");
                return;
            }

            Transform container = listItemPrefab.transform.parent;

            foreach (Transform child in container.GetChildren())
            {
                if (child == listItemPrefab.transform || !child.name.StartsWith(listItemPrefab.name))
                    continue;
                Object.Destroy(child.gameObject);
            }

            foreach (Station station in Object.FindObjectsOfType<Station>())
            {
                GameObject name = Object.Instantiate(listItemPrefab, container);
                name.FindChildByName("Color").GetComponent<Image>().color = station.color;
                name.FindChildByName("IndustryCode").GetComponent<TMP_Text>().text = station.stationID;
                name.FindChildByName("OriginalName").GetComponent<TMP_Text>().text = station.stationName;
                GameObject localizedName = name.FindChildByName("LocalizedName");
                localizedName.GetComponent<TMP_Text>().text = station.stationName;
                foreach (Localize i2Localize in localizedName.GetComponents<Localize>())
                    Object.DestroyImmediate(i2Localize);
                Object.DestroyImmediate(localizedName.GetComponent<DV.Localization.Localize>());
            }

            Object.Destroy(listItemPrefab.gameObject);
        }

        public static void UpdateMap(Transform transform)
        {
            if (!modifiedMaterial)
            {
                Transform mapObject = transform.FindChildByName("Map_LOD0");
                if (mapObject == null)
                {
                    Mapify.LogError($"Failed to find 'Map_LOD0' under '{transform.name}'!");
                    return;
                }

                Material material = mapObject.GetComponent<Renderer>().sharedMaterial;
                if (defaultTexture == null)
                    defaultTexture = material.mainTexture;
                material.mainTexture = Maps.IsDefaultMap ? defaultTexture : DrawTracksOnMap();
                modifiedMaterial = true;
            }

            Transform names = transform.FindChildByName("Names");
            if (names == null)
            {
                Mapify.LogError($"Failed to find 'Names' under '{transform.name}'!");
                return;
            }

            Transform[] children = names.GetChildren();
            if (children.Length == 0)
            {
                Mapify.LogError($"Map 'Names' under '{transform.name}' has no children!");
                return;
            }

            GameObject namePrefab = children[0].gameObject;

            if (children.Length > 1)
                for (int i = 1; i < children.Length; i++)
                {
                    if (children[i].name == "Legend") continue;
                    Object.Destroy(children[i].gameObject);
                }

            foreach (Station station in Object.FindObjectsOfType<Station>())
            {
                GameObject name = Object.Instantiate(namePrefab, names);
                TMP_Text tmp = name.GetComponent<TMP_Text>();
                tmp.rectTransform.localPosition = station.YardCenter.position.ToXZ().Scale(0, Maps.LoadedMap.worldSize, -0.175f, 0.175f);

                if (showStationNamesOnMap)
                {
                    tmp.text = station.stationName;
                }
                else
                {
                    tmp.text = station.stationID;
                }
            }

            Object.Destroy(namePrefab);
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
    }
}
