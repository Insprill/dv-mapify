using System.Collections.Generic;
using System.Linq;
using DV;
using DV.Utils;
using UnityEngine;

namespace Mapify.BuildMode
{
    public class BuildMode: MonoBehaviour
    {
        private bool placeMode = false;
        private GameObject previewObject;
        private GameObject originalObject;
        private int assetIndex = 0;
        private List<GameObject> placedGameObjects;

        private const int LEFT_MOUSE_BUTTON = 0;

        private void Awake()
        {
            placedGameObjects = new List<GameObject>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.M))
            {
                TogglePlaceMode();
            }

            //TODO check application window focus
            if (placeMode && !SingletonBehaviour<AppUtil>.Instance.IsPauseMenuOpen)
            {
                UpdatePlaceMode();
            }
        }

        private void UpdatePlaceMode()
        {
            var scrollDelta = KeyBindings.GetScrolling();
            if (scrollDelta != 0)
            {
                assetIndex += scrollDelta;
                assetIndex = BetterModulo(assetIndex, BuildingAssetsRegistry.Assets.Count);

                SelectObject();
            }

            ShowPreview();
        }

        private void ShowPreview()
        {
            var ray = Camera.current.ScreenPointToRay(Input.mousePosition);

            if (!Physics.Raycast(ray, out RaycastHit hit, 150f))
            {
                previewObject.SetActive(false);
                return;
            }

            previewObject.SetActive(true);
            previewObject.transform.position = hit.point;

            if (Input.GetMouseButtonDown(LEFT_MOUSE_BUTTON))
            {
                PlaceObject(hit.point);
            }
        }

        private void PlaceObject(Vector3 position)
        {
            Mapify.LogDebug(() => $"Placing '{originalObject.name}' at {position}");
            var placed = Instantiate(originalObject, position, previewObject.transform.rotation);
            placed.SetActive(true);
            placedGameObjects.Add(placed);
        }

        private void TogglePlaceMode()
        {
            if (placeMode)
            {
                ExitPlaceMode();
            }
            else
            {
                EnterPlaceMode();
            }
        }

        private void EnterPlaceMode()
        {
            placeMode = true;
            SelectObject();
        }

        private void ExitPlaceMode()
        {
            placeMode = false;
            Destroy(previewObject);
        }

        private void SelectObject()
        {
            var keysList = BuildingAssetsRegistry.Assets.Keys.ToList();
            var newKey = keysList[assetIndex];

            originalObject = BuildingAssetsRegistry.Assets[newKey];

            Destroy(previewObject);
            previewObject = Instantiate(originalObject);
            foreach (var collider in previewObject.GetComponentsInChildren<Collider>())
            {
                DestroyImmediate(collider);
            }
        }

        //Like the modulo operator but it works with negative numbers
        public static int BetterModulo(int x, int m)
        {
            return (x % m + m) % m;
        }
    }
}
