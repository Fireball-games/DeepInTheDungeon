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
        public static string MapDirectoryPath => Path.Combine(ApplicationPath, MapDirectoryName);
        
        private const string MapFileExtension = ".map";
        private static readonly string ApplicationPath = Application.persistentDataPath;

        public static string[] GetFilesInDirectory(string relativeDirectoryPath = "", string extensionFilter = "all")
        {
            string fullPath = Path.Combine(ApplicationPath, relativeDirectoryPath);

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

            return !File.Exists(GetFullMapPath(mapName)) 
                ? null 
                : ES3.Load<MapDescription>(mapName, GetFullRelativeMapPath(mapName));
        }

        private static bool LoadPrefabs(EPrefabType prefabType, out IEnumerable<GameObject> loadedPrefabs)
        {
            loadedPrefabs = null;
            return true;
        }

        public static string GetFullRelativeMapPath(string mapName) => Path.Combine(MapDirectoryName, $"{mapName}{MapFileExtension}");

        public static string GetFullMapPath(string mapName) => Path.Combine(MapDirectoryPath, $"{mapName}{MapFileExtension}");
    }
}