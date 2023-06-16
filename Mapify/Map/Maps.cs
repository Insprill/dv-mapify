using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Mapify.Editor;
using UnityEngine;

namespace Mapify.Map
{
    public static class Maps
    {
        private const string MAPS_FOLDER_NAME = "Maps";
        public static readonly BasicMapInfo DEFAULT_MAP_INFO = new BasicMapInfo(Names.DEFAULT_MAP_NAME, null);
        private static readonly string[] requiredFiles = { Names.MAP_INFO_FILE, Names.ASSETS_ASSET_BUNDLE, Names.SCENES_ASSET_BUNDLE };

        public static bool IsDefaultMap { get; private set; } = true;
        private static MapInfo _loadedMap;

        public static MapInfo LoadedMap {
            get {
                if (_loadedMap == null)
                    throw new InvalidOperationException("No map has been loaded!");
                return _loadedMap;
            }
            private set => _loadedMap = value;
        }

        public static string[] AllMapNames { get; private set; }
        private static string MapsFolder;
        // name -> (info, directory)
        private static ReadOnlyDictionary<string, (BasicMapInfo, string)> availableMaps;

        public static void LoadMaps(Mapify plugin)
        {
            MapsFolder = Path.Combine(plugin.InstallDirectory, MAPS_FOLDER_NAME);
            if (!Directory.Exists(MapsFolder))
            {
                Directory.CreateDirectory(MapsFolder);
            }
            else
            {
                Mapify.Log("Searching for maps...");
                FindMaps();
                Mapify.Log($"Found {availableMaps.Count} map(s) ({string.Join(", ", availableMaps.Keys.ToArray())})");
            }
        }

        private static void FindMaps()
        {
            Dictionary<string, (BasicMapInfo, string)> maps = new Dictionary<string, (BasicMapInfo, string)> {
                { DEFAULT_MAP_INFO.mapName, (DEFAULT_MAP_INFO, null) }
            };

            foreach (string dir in Directory.GetDirectories(MapsFolder))
            {
                if (!ValidateMapInstallation(dir))
                    continue;
                string mapInfoPath = GetMapAsset(Names.MAP_INFO_FILE, dir);
                BasicMapInfo mapInfo = JsonUtility.FromJson<BasicMapInfo>(File.ReadAllText(mapInfoPath));
                if (mapInfo.mapName == Names.DEFAULT_MAP_NAME)
                {
                    Mapify.LogError($"Skipping map in {dir} due to restricted name: {Names.DEFAULT_MAP_NAME}");
                    continue;
                }

                maps.Add(mapInfo.mapName, (mapInfo, dir));
            }

            availableMaps = new ReadOnlyDictionary<string, (BasicMapInfo, string)>(maps);
            AllMapNames = availableMaps.Keys?.OrderBy(x => x).ToArray() ?? Array.Empty<string>();
        }

        private static bool ValidateMapInstallation(string dir)
        {
            foreach (string requiredFile in requiredFiles)
            {
                if (File.Exists(GetMapAsset(requiredFile, dir)))
                    continue;
                Mapify.LogError($"Failed to find file '{requiredFile}' for map in '{dir}'!");
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Gets a <see cref="BasicMapInfo" /> from it's alphabetical index in <see cref="AllMapNames" />.
        ///     Indexes are not applicable between game restarts!
        /// </summary>
        public static BasicMapInfo FromIndex(int index)
        {
            return availableMaps[AllMapNames[index]].Item1;
        }

        public static string GetDirectory(BasicMapInfo basicMapInfo)
        {
            return availableMaps[basicMapInfo.mapName].Item2;
        }

        public static void RegisterLoadedMap(MapInfo mapInfo)
        {
            IsDefaultMap = mapInfo.mapName == DEFAULT_MAP_INFO.mapName;
            LoadedMap = mapInfo;
        }

        public static string GetMapAsset(string fileName, string mapDir = null)
        {
            return Path.Combine(MapsFolder, mapDir ?? availableMaps[LoadedMap.mapName].Item2, fileName);
        }
    }
}
