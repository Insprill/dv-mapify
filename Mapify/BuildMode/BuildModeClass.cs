using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DV;
using DV.UI;
using DV.Utils;
using Mapify.Editor.Utils;
using RuntimeHandle;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;

namespace Mapify.BuildMode
{
    public class BuildModeClass: MonoBehaviour
    {
        private bool placeMode = false;
        private bool hasSelectedAnObject = false;
        private bool mouseModeWasEnabled;

        private GameObject previewObject;
        private GameObject originalObject;
        private List<GameObject> placedGameObjects;

        private static GameObject assetMenuPrefab;
        private static GameObject assetAreaObjectPrefab;
        private GameObject assetMenu;

        private const int LEFT_MOUSE_BUTTON = 0;
        private const int RIGHT_MOUSE_BUTTON = 1;

        private void Awake()
        {
            placedGameObjects = new List<GameObject>();
        }

        private void Start()
        {
            SetupAssetSelectMenu();
        }

        private void SetupAssetSelectMenu()
        {
            assetMenu = Instantiate(assetMenuPrefab);
            var assetAreaObject = assetMenu.GetComponentInChildren<GridLayoutGroup>().gameObject;

            if (assetAreaObject == null)
            {
                Mapify.LogError("Could not find asset area object");
                return;
            }

            var assetAreaScript = assetAreaObject.AddComponent<AssetArea>();

            //TODO real image
            var placeHolderTexture = FindObjectOfType<Texture2D>();
            if (placeHolderTexture == null)
            {
                Mapify.LogError("Could not find Texture2D");
            }

            if (!BuildingAssetsRegistry.Assets.Any())
            {
                Mapify.LogError("BuildingAssetsRegistry.Assets is empty");
            }

            foreach (var ass in BuildingAssetsRegistry.Assets)
            {
                assetAreaScript.CreateAreaObject(
                    assetAreaObjectPrefab,
                    ass.Key,
                    placeHolderTexture,
                    () => { OnAssetClicked(ass.Key); }
                );
            }

            assetMenu.SetActive(false);
        }

        private void OnAssetClicked(string assetName)
        {
            SelectObject(BuildingAssetsRegistry.Assets[assetName]);
            assetMenu.SetActive(false);
        }

        private void Update()
        {
            if (!Application.isFocused)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.M))
            {
                TogglePlaceMode();
            }

            if (placeMode)
            {
                UpdatePlaceMode();
            }
        }

        private static bool MouseModeIsEnabled()
        {
            return SingletonBehaviour<ACanvasController<CanvasController.ElementType>>.Instance.IsOn(CanvasController.ElementType.MouseMode);
        }

        private static void SetMouseMode(bool enableMouseMode)
        {
            SingletonBehaviour<ACanvasController<CanvasController.ElementType>>.Instance.TrySetState(CanvasController.ElementType.MouseMode, enableMouseMode);
        }

        private void UpdatePlaceMode()
        {
            if (Input.GetKeyDown(KeyCode.Comma))
            {
                ToggleAssetSelectMenu();
            }

            if (!SingletonBehaviour<AppUtil>.Instance.IsPauseMenuOpen &&
                !assetMenu.activeSelf &&
                hasSelectedAnObject)
            {
                ShowPreview();
            }
        }

        private void ToggleAssetSelectMenu()
        {
            assetMenu.SetActive(!assetMenu.activeSelf);

            if (assetMenu.activeSelf)
            {
                //entered the menu
                mouseModeWasEnabled = MouseModeIsEnabled();
                SetMouseMode(true);
            }
            else
            {
                //exited the menu
                SetMouseMode(mouseModeWasEnabled);
            }
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

            RuntimeTransformHandle.Create(placed.transform, HandleType.POSITION);
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
            hasSelectedAnObject = false;
        }

        private void ExitPlaceMode()
        {
            placeMode = false;
            previewObject.SetActive(false);
            assetMenu.SetActive(false);
        }

        private void SelectObject(GameObject newObject)
        {
            originalObject = newObject;

            Destroy(previewObject);
            previewObject = Instantiate(originalObject);
            foreach (var collider in previewObject.GetComponentsInChildren<Collider>())
            {
                DestroyImmediate(collider);
            }

            hasSelectedAnObject = true;
        }

        public static void LoadAssets(AssetBundle assetBundle, string assetsFolderPath)
        {
            assetMenuPrefab = assetBundle
                .LoadAsset<GameObject>($"{assetsFolderPath}menu_canvas.prefab");

            if (assetMenuPrefab == null)
            {
                throw new Exception("Failed to load menucanvas.prefab");
            }

            assetAreaObjectPrefab = assetBundle
                .LoadAsset<GameObject>($"{assetsFolderPath}area_object.prefab");

            if (assetAreaObjectPrefab == null)
            {
                throw new Exception("Failed to load area_object.prefab");
            }
        }
    }
}
