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
        private bool inBuildMode = false;
        private bool inPlaceMode = false;
        private bool mouseModeWasEnabled;
        private bool initialized = false;

        private bool hasSelectedAnObject = false;
        private GameObject previewObject;
        private GameObject originalObject;

        private List<PlacedAsset> placedAssetsList = new List<PlacedAsset>();
        private HandleManager handleManager;

        private static GameObject assetMenuPrefab;
        private static GameObject assetAreaObjectPrefab;
        private GameObject assetMenu;

        //This is used, don't let Rider tell you otherwise.
        public new static string AllowAutoCreate() => "[BuildMode]";

        protected override void Awake()
        {
            base.Awake();
            handleManager = gameObject.AddComponent<HandleManager>();
        }

        private void OnDisable()
        {
            placedAssetsList = new List<PlacedAsset>();
            Destroy(handleManager);
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

            var assetArea = assetAreaObject.AddComponent<AssetArea>();

            if (!BuildingAssetsRegistry.Assets.Any())
            {
                Mapify.LogError("BuildingAssetsRegistry.Assets is empty");
            }

            foreach (var ass in BuildingAssetsRegistry.Assets)
            {
                var texture = BuildingAssetsRegistry.AssetImages.ContainsKey(ass.Key) ? BuildingAssetsRegistry.AssetImages[ass.Key] : Texture2D.whiteTexture;

                assetArea.CreateAreaObject(
                    assetAreaObjectPrefab,
                    ass.Key,
                    texture,
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
            if (!inPlaceMode)
            {
                EnterPlaceMode();
            }
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
                ToggleBuildMode();
            }

            if (!inBuildMode) { return; }

            if (Input.GetKeyDown(KeyCode.Comma))
            {
                ToggleAssetSelectMenu();
            }

            CheckHandleControls();

            if (inPlaceMode)
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
            if (!hasSelectedAnObject || assetMenu.activeSelf) return;

            if (Input.GetMouseButtonDown(Constants.RIGHT_MOUSE_BUTTON))
            {
                hasSelectedAnObject = false;
                previewObject.SetActive(false);
            }
            else
            {
                ShowPreview();
            }
        }

        public void CheckHandleControls()
        {
            if (assetMenu.activeSelf) return;

            //TODO make these rebindable
            if (Input.GetKeyDown(KeyCode.P) && !inPlaceMode)
            {
                EnterPlaceMode();
            }
            if (Input.GetKeyDown(KeyCode.G))
            {
                handleManager.SetHandleTypes(HandleType.POSITION);
                ExitPlaceMode();
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                handleManager.SetHandleTypes(HandleType.ROTATION);
                ExitPlaceMode();
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

            handleManager.Add(placed.transform);
        }

        private void EnterPlaceMode()
        {
            inPlaceMode = true;
            if (hasSelectedAnObject)
            {
                previewObject.SetActive(true);
            }
            handleManager.SetHandlesActive(false);
        }

        private void ExitPlaceMode()
        {
            inPlaceMode = false;
            previewObject.SetActive(false);
            handleManager.SetHandlesActive(true);
        }

        private void ToggleBuildMode()
        {
            if (inBuildMode)
            {
                ExitBuildMode();
            }
            else
            {
                EnterBuildMode();
            }
        }

        private void EnterBuildMode()
        {
            if (inPlaceMode)
            {
                EnterPlaceMode();
            }
            else
            {
                handleManager.SetHandlesActive(true);
            }
            inBuildMode = true;
        }

        private void ExitBuildMode()
        {
            previewObject.SetActive(false);
            inBuildMode = false;
            assetMenu.SetActive(false);
            handleManager.SetHandlesActive(false);
        }

        private void SelectObject(GameObject newObject)
        {
            originalObject = newObject;

            Destroy(previewObject);
            previewObject = Instantiate(originalObject);
            foreach (var collider in previewObject.GetComponentsInChildren<Collider>())
            {
                Destroy(collider);
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
