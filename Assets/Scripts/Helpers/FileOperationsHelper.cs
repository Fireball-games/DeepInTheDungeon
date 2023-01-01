using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Scripts.Building;
using UnityEngine;
using static Scripts.Enums;

namespace Scripts.Helpers
{
    public static class FileOperationsHelper
    {
        public const string MapDirectoryName = "Maps";
        public const string WallsDirectoryName = "Walls";
        public const string EnemiesDirectoryName = "Enemies";
        public const string PropsDirectoryName = "Props";
        public const string ItemsDirectoryName = "Items";
        public const string PrefabsDirectoryName = "Prefabs";
        
        public static string MapDirectoryPath => Path.Combine(PersistentPath, MapDirectoryName);
        
        private const string MapFileExtension = ".map";
        private static readonly string PersistentPath = Application.persistentDataPath;

        public static string[] GetFilesInDirectory(string relativeDirectoryPath = "", string extensionFilter = "all")
        {
            string fullPath = Path.Combine(PersistentPath, relativeDirectoryPath);

            string[] allFiles = !Directory.Exists(fullPath) ? null : Directory.GetFiles(fullPath);

            if (allFiles == null) return null;

            if (extensionFilter != "all")
            {
                allFiles = allFiles.Where(file => Path.GetExtension(file) == extensionFilter).ToArray();
            }

            return allFiles;
        }

        public static string GetSavePath(string mapName)
        {
            return Path.Combine(MapDirectoryName, $"{mapName}.map");
        }

        public static MapDescription LoadLastPlayedMap()
        {
            string mapName = PlayerPrefs.GetString(Strings.LastPlayedMap, null);

            if (string.IsNullOrEmpty(mapName)) return null;

            if (!File.Exists(GetFullMapPath(mapName)))
            {
                return null;
            }

            try
            {
                return ES3.Load<MapDescription>(mapName, GetFullRelativeMapPath(mapName));
            }
            catch (Exception e)
            {
                Logger.Log($"Failed to load level from file: {mapName}: {e}", Logger.ELogSeverity.Release);
                return null;
            }
        }

        public static bool LoadPrefabs(EPrefabType prefabType, out HashSet<GameObject> loadedPrefabs)
        {
            loadedPrefabs = Resources.LoadAll<GameObject>(GetPrefabPathByType(prefabType)).ToHashSet();

            return loadedPrefabs != null && loadedPrefabs.Any();
        }

        private static string GetPrefabPathByType(EPrefabType prefabType) => prefabType switch
        {
            EPrefabType.Wall => WallsDirectoryName,
            EPrefabType.WallBetween => WallsDirectoryName,
            EPrefabType.WallOnWall => WallsDirectoryName,
            EPrefabType.WallForMovement => WallsDirectoryName,
            EPrefabType.PrefabTile => PrefabsDirectoryName,
            EPrefabType.Enemy => EnemiesDirectoryName,
            EPrefabType.Prop => PropsDirectoryName,
            EPrefabType.Item => ItemsDirectoryName,
            _ => throw new ArgumentOutOfRangeException(nameof(prefabType), prefabType, null)
        };

        private static string GetFullRelativeMapPath(string mapName) => Path.Combine(MapDirectoryName, $"{mapName}{MapFileExtension}");

        private static string GetFullMapPath(string mapName) => Path.Combine(MapDirectoryPath, $"{mapName}{MapFileExtension}");
    }
}