using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DV;
using Mapify.Editor;
using UnityEngine;
using UnityModManagerNet;

namespace Mapify.Map
{
    public static class Maps
    {
        public static readonly BasicMapInfo DEFAULT_MAP_INFO = new BasicMapInfo(Names.DEFAULT_MAP_NAME, $"{BuildInfo.BUILD_VERSION_MAJOR}", null);

        public static Action OnMapsUpdated;
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

        public static string[] AllMapNames { get; private set; } = { DEFAULT_MAP_INFO.name };
        /// <summary>
        ///     name -> (info, mod, directory)
        /// </summary>
        private static readonly Dictionary<string, (BasicMapInfo, UnityModManager.ModEntry, string)> availableMaps = new Dictionary<string, (BasicMapInfo, UnityModManager.ModEntry, string)> {
            { DEFAULT_MAP_INFO.name, (DEFAULT_MAP_INFO, null, null) }
        };

        public static void Init()
        {
            UnityModManager.toggleModsListen += ToggleModsListen;
            foreach (UnityModManager.ModEntry entry in UnityModManager.modEntries)
                FindMaps(entry);
        }

        private static void ToggleModsListen(UnityModManager.ModEntry modEntry, bool result)
        {
            if (result)
            {
                Mapify.LogDebug(() => $"New mod enabled ({modEntry.Info.DisplayName}), checking for maps...");
                FindMaps(modEntry);
                return;
            }

            foreach ((BasicMapInfo, UnityModManager.ModEntry, string) map in availableMaps.Values)
            {
                if (map.Item2 != modEntry)
                    continue;
                if (_loadedMap == null || _loadedMap.name != map.Item1.name)
                    continue;
                Mapify.LogWarning($"Tried to disable mod ({modEntry.Info.DisplayName}) with map ({map.Item1.name}) loaded. Re-enabling mod!");
                modEntry.Load();
                break;
            }
        }

        private static void FindMaps(UnityModManager.ModEntry modEntry)
        {
            bool foundMap = false;
            foreach (string dir in Directory.GetDirectories(modEntry.Path))
            {
                string mapInfoPath = GetMapAsset(Names.MAP_INFO_FILE, dir);
                if (!File.Exists(mapInfoPath))
                    continue;

                BasicMapInfo mapInfo = JsonUtility.FromJson<BasicMapInfo>(File.ReadAllText(mapInfoPath));
                if (mapInfo.name == Names.DEFAULT_MAP_NAME)
                {
                    Mapify.LogError($"Skipping map in '{dir}' due to restricted name: '{Names.DEFAULT_MAP_NAME}'");
                    continue;
                }

                if (availableMaps.TryGetValue(mapInfo.name, out (BasicMapInfo, UnityModManager.ModEntry, string) existingMap))
                {
                    if (existingMap.Item2 != modEntry)
                        Mapify.LogError($"Skipping map from '{modEntry.Info.DisplayName}' in '{dir}' due to duplicate name '{mapInfo.name}' (Already added by '{existingMap.Item2.Info.DisplayName}')");
                    continue;
                }

                Mapify.LogDebug(() => $"Found map '{mapInfo.name}' from '{modEntry.Info.DisplayName}' in '{dir}'");
                availableMaps.Add(mapInfo.name, (mapInfo, modEntry, dir));
                foundMap = true;
            }

            if (!foundMap)
                return;
            AllMapNames = availableMaps.Keys.OrderBy(x => x).ToArray();
            OnMapsUpdated?.Invoke();
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
            return availableMaps[basicMapInfo.name].Item3;
        }

        public static void RegisterLoadedMap(MapInfo mapInfo)
        {
            IsDefaultMap = mapInfo.name == DEFAULT_MAP_INFO.name;
            LoadedMap = mapInfo;
        }

        public static void UnregisterLoadedMap()
        {
            IsDefaultMap = true;
            LoadedMap = null;
        }

        public static string GetMapAsset(string fileName, string mapDir = null)
        {
            return Path.Combine(mapDir ?? availableMaps[LoadedMap.name].Item3, fileName);
        }

        public static string[] GetMapAssets(string searchPattern, string mapDir = null)
        {
            string path = mapDir ?? availableMaps[LoadedMap.name].Item3;
            return Directory.GetFiles(path, searchPattern);
        }
    }
}
