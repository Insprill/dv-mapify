using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using DV;
using DV.UI;
using DV.UserManagement;
using DV.UserManagement.Storage.Implementation;
using DV.Utils;
using RuntimeHandle;
using UnityEngine;
using UnityEngine.UI;

namespace Mapify.BuildMode
{
    public class BuildModeClass: SingletonBehaviour<BuildModeClass>
    {
        private bool placeMode = false;
        private bool hasSelectedAnObject = false;
        private bool mouseModeWasEnabled;
        private bool initialized = false;

        private GameObject previewObject;
        private GameObject originalObject;

        private List<PlacedAsset> placedAssetsList = new List<PlacedAsset>();
        private List<RuntimeTransformHandle> handles = new List<RuntimeTransformHandle>();

        private static GameObject assetMenuPrefab;
        private static GameObject assetAreaObjectPrefab;
        private GameObject assetMenu;

        //This is used, don't let Rider tell you otherwise.
        public new static string AllowAutoCreate() => "[BuildMode]";

        private void OnDisable()
        {
            placedAssetsList = new List<PlacedAsset>();
            foreach (var h in handles)
            {
                Destroy(h);
            }
            initialized = false;
        }

        public void LoadPlacedAssets(string xmlPath)
        {
            if (!File.Exists(xmlPath))
            {
                Mapify.LogDebug(() => $"{nameof(LoadPlacedAssets)}: Skipping XML file {xmlPath}, it doesn't exist.");
                return;
            }

            try
            {
                using (var fileStream = File.OpenRead(xmlPath))
                {
                    placedAssetsList = (List<PlacedAsset>) new XmlSerializer(typeof(List<PlacedAsset>)).Deserialize(fileStream);
                    foreach (var placedAsset in placedAssetsList)
                    {
                        try
                        {
                            PlaceObject(BuildingAssetsRegistry.Assets[placedAsset.Name], placedAsset.Position, placedAsset.Rotation);
                        }
                        catch (KeyNotFoundException)
                        {
                            Mapify.LogError($"{nameof(LoadPlacedAssets)}: key {placedAsset.Name} not in {nameof(BuildingAssetsRegistry.Assets)}");
                        }
                    }

                    Mapify.LogDebug(() => $"{nameof(LoadPlacedAssets)}: loaded {placedAssetsList.Count} assets");
                }
            }
            catch (Exception e)
            {
                Mapify.LogException($"Failed to load XML at {xmlPath}", e);
            }
        }

        //TODO object moved after placing is not taken into account
        public void SavePlacedAssets(string xmlPath)
        {
            Mapify.LogDebug(() => $"{nameof(SavePlacedAssets)}: Saving XML file {xmlPath}");

            try
            {
                using (var fileStream = File.OpenWrite(xmlPath))
                {
                    new XmlSerializer(typeof(List<PlacedAsset>)).Serialize(fileStream, placedAssetsList);
                }
            }
            catch (Exception e)
            {
                Mapify.LogException($"Failed to save XML at {xmlPath}", e);
            }
        }

        public void SetupAssetSelectMenu()
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

            initialized = true;
        }

        private void OnAssetClicked(string assetName)
        {
            SelectObject(BuildingAssetsRegistry.Assets[assetName]);
            assetMenu.SetActive(false);
        }

        private void Update()
        {
            if (!Application.isFocused ||
                !initialized ||
                SingletonBehaviour<AppUtil>.Instance.IsPauseMenuOpen)
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

            if (assetMenu.activeSelf) return;

            CheckHandleControls();

            if (Input.GetMouseButtonDown(Constants.RIGHT_MOUSE_BUTTON))
            {
                hasSelectedAnObject = false;
                previewObject.SetActive(false);
            }

            if (hasSelectedAnObject)
            {
                ShowPreview();
            }
        }

        public void CheckHandleControls()
        {
            if (Input.GetKeyDown(KeyCode.G))
            {
                SetHandleTypes(HandleType.POSITION);
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                SetHandleTypes(HandleType.ROTATION);
            }
        }

        private void SetHandleTypes(HandleType handleType)
        {
            foreach (var h in handles)
            {
                h.SetHandleMode(handleType);
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

            if (!Physics.Raycast(ray, out RaycastHit hit, 500f))
            {
                previewObject.SetActive(false);
                return;
            }

            previewObject.SetActive(true);
            previewObject.transform.position = hit.point;

            if (Input.GetMouseButtonDown(Constants.LEFT_MOUSE_BUTTON))
            {
                PlaceObject(originalObject, hit.point, previewObject.transform.rotation);
                placedAssetsList.Add(new PlacedAsset(originalObject.name, hit.point, previewObject.transform.rotation));
            }
        }

        private void PlaceObject(GameObject objectToPlace, Vector3 position, Quaternion rotation)
        {
            Mapify.LogDebug(() => $"Placing '{objectToPlace.name}' at {position} rotation {rotation.eulerAngles}");
            var placed = Instantiate(objectToPlace, position, rotation);
            placed.name = objectToPlace.name;
            placed.transform.SetParent(WorldMover.Instance.transform);
            placed.SetActive(true);

            handles.Add(RuntimeTransformHandle.Create(placed.transform, HandleType.POSITION));
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

        public static string GetDefaultMapXMLPath()
        {
            var storage = (FileSystemStorage)SingletonBehaviour<UserManager>.Instance.storage;
            return Path.Combine(storage.basePath, Constants.PLACED_ASSETS_XML);
        }
    }
}
