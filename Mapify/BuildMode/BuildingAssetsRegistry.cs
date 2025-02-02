using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Mapify.Editor.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Mapify.BuildMode
{
    public static class BuildingAssetsRegistry
    {
        public static SortedDictionary<string, GameObject> Assets => assets;
        private static SortedDictionary<string, GameObject> assets = new SortedDictionary<string, GameObject>();

        public static SortedDictionary<string, Texture2D> AssetImages => assetImages;
        private static SortedDictionary<string, Texture2D> assetImages = new SortedDictionary<string, Texture2D>();

        private static GameObject registryMainObject;
        private static List<string> searchedScenes = new List<string>();

        private static readonly Regex CLONE_PATTERN = new Regex(@"\(\d+\)$"); //matches 'blabla (1)' and not 'blabla'

        private const int SCREENSHOT_LAYER = 11;
        private const string CACHE_FOLDER_NAME = "AssetImagesCache";

        public static void OnSceneLoad(Scene scene, LoadSceneMode mode)
        {
            // Mapify map scenes have a buildIndex of -1. We only want to copy assets from the default game, so we skip Mapify scenes.
            if(scene.buildIndex == -1){ return; }

            //already handled this scene
            if (searchedScenes.Contains(scene.name)) { return; }

            if (registryMainObject == null)
            {
                registryMainObject = new GameObject("[BuildingAssets]");
            }

            searchedScenes.Add(scene.name);
            Mapify.Log("RegisterAssets "+scene.name);

            var camera = CreatePreviewGeneratorCamera();

            foreach (var rootObject in scene.GetRootGameObjects())
            {
                foreach (var lod in rootObject.GetComponentsInChildren<LODGroup>(true))
                {
                    var originalObject = lod.gameObject;

                    //TODO Names of objects might not be unique. Is there a better way to determine whether 2 GameObjects are of the same asset?
                    //avoid duplicates
                    if (CLONE_PATTERN.IsMatch(originalObject.name) || assets.ContainsKey(originalObject.name))
                    {
                        continue;
                    }

                    RegisterAsset(originalObject, camera);
                }
            }

            Object.Destroy(camera.gameObject);
        }

        private static Camera CreatePreviewGeneratorCamera()
        {
            var cameraObject = new GameObject();
            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.clear;
            camera.cullingMask = 1 << SCREENSHOT_LAYER;
            camera.orthographic = true;
            return camera;
        }

        private static void RegisterAsset(GameObject originalObject, Camera previewGeneratorCamera)
        {
            var copy = Object.Instantiate(originalObject, registryMainObject.transform);
            copy.name = originalObject.name;
            copy.SetActive(false);
            assets.Add(copy.name, copy);

            if (!assetImages.ContainsKey(originalObject.name))
            {
                SetupAssetImage(originalObject, previewGeneratorCamera);
            }
        }

        private static void SetupAssetImage(GameObject originalObject, Camera previewGeneratorCamera)
        {
            var folder = Path.Combine(Mapify.ModEntry.Path, CACHE_FOLDER_NAME);
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            var path = Path.Combine(folder, originalObject.name + ".png");

            if (Mapify.MySettings.UseAssetImagesCache && File.Exists(path))
            {
                assetImages.Add(originalObject.name, LoadImage(path));
            }
            else
            {
                assetImages.Add(originalObject.name, GenerateImage(originalObject, path, previewGeneratorCamera));
            }
        }

        private static Texture2D LoadImage(string path)
        {
            Mapify.LogDebug(() => $"{nameof(LoadImage)}: {path}");

            var fileData = File.ReadAllBytes(path);
            var texture = new Texture2D(2, 2);
            texture.LoadImage(fileData); //this will auto-resize the texture dimensions.
            return texture;
        }

        private static Texture2D GenerateImage(GameObject original, string path, Camera camera)
        {
            Mapify.LogDebug(() => $"{nameof(GenerateImage)}: {original.name}, {path}");

            var copy = Object.Instantiate(original, Vector3.zero, Quaternion.identity);
            copy.SetActive(true);
            copy.SetLayersRecursive(SCREENSHOT_LAYER);

            Focus(camera, copy);
            var screenshot = TakeScreenshot(camera, path);

            // Unity docs say to use Destroy and not DestroyImmediate, but with Destroy this does not work.
            Object.DestroyImmediate(copy);
            return screenshot;
        }

        private static void Focus(Camera camera_, GameObject target)
        {
            FindSizeAndCenter(target, out Vector3 size, out Vector3 center);

            var width = size.x;
            var height = size.y;
            var depth = size.z;

            camera_.transform.rotation = Quaternion.Euler(0, 180, 0);
            camera_.transform.position = center + new Vector3(0, 0, depth*2);

            camera_.orthographicSize = (width > height * camera_.aspect ? width / camera_.pixelWidth * camera_.pixelHeight : height) / 2;
            //padding
            // camera_.orthographicSize += width > height ? width * 0.05f : height * 0.05f;
        }

        private static void FindSizeAndCenter(GameObject target, out Vector3 size, out Vector3 center)
        {
            var minimum = Vector3.zero;
            var maximum = Vector3.zero;
            bool set = false;

            var renderers = target.GetComponentsInChildren<MeshRenderer>(true);
            if (!renderers.Any())
            {
                Mapify.LogError($"{nameof(BuildingAssetsRegistry)}.{nameof(Focus)}: could not find renderer on target {target.name}");
                size = Vector3.zero;
                center = Vector3.zero;
                return;
            }

            foreach (var meshRenderer in renderers)
            {
                if (!set)
                {
                    minimum = meshRenderer.bounds.min;
                    maximum = meshRenderer.bounds.max;
                    set = true;
                    continue;
                }

                if (meshRenderer.bounds.min.x < minimum.x)
                {
                    minimum.x = meshRenderer.bounds.min.x;
                }
                if (meshRenderer.bounds.min.y < minimum.y)
                {
                    minimum.y = meshRenderer.bounds.min.y;
                }
                if (meshRenderer.bounds.min.z < minimum.z)
                {
                    minimum.z = meshRenderer.bounds.min.z;
                }

                if (meshRenderer.bounds.max.x > maximum.x)
                {
                    maximum.x = meshRenderer.bounds.max.x;
                }
                if (meshRenderer.bounds.max.y > maximum.y)
                {
                    maximum.y = meshRenderer.bounds.max.y;
                }
                if (meshRenderer.bounds.max.z > maximum.z)
                {
                    maximum.z = meshRenderer.bounds.max.z;
                }
            }

            size = maximum - minimum;
            center = minimum + 0.5f * size;
        }

        //source https://stackoverflow.com/a/64552656
        private static Texture2D TakeScreenshot(Camera camera_, string path)
        {
            var bak_RenderTexture_active = RenderTexture.active;

            var width = 200;
            var height = 200;

            var texture_transparent = new Texture2D(width, height, TextureFormat.ARGB32, false);
            // Must use 24-bit depth buffer to be able to fill background.
            var render_texture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);
            var grab_area = new Rect(0, 0, width, height);

            RenderTexture.active = render_texture;
            camera_.targetTexture = render_texture;

            camera_.Render();
            texture_transparent.ReadPixels(grab_area, 0, 0);
            texture_transparent.Apply();

            // Encode the resulting output texture to a byte array then write to the file
            var pngShot = texture_transparent.EncodeToPNG();
            File.WriteAllBytes(path, pngShot);

            RenderTexture.active = bak_RenderTexture_active;
            RenderTexture.ReleaseTemporary(render_texture);

            return texture_transparent;
        }

        public static void FinishRegistering()
        {
            Mapify.LogDebug(() => $"{nameof(FinishRegistering)}: assets({assets.Count}):");

            foreach (var ass in assets)
            {
                if (ass.Value == null)
                {
                    Mapify.LogError($"{nameof(FinishRegistering)}: found null asset {ass.Key}");
                    continue;
                }
                Mapify.LogDebug(() => ass.Value.name);
            }

            BuildModeClass.Instance.SetupAssetSelectMenu();
        }

        public static void CleanUp()
        {
            foreach (var ass in assets)
            {
                Object.Destroy(ass.Value);
            }

            assets.Clear();
            Object.Destroy(registryMainObject);
        }
    }
}
