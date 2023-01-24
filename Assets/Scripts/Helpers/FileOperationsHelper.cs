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
        public const string CampaignDirectoryName = "Maps";
        public const string WallsDirectoryName = "Walls";
        public const string EnemiesDirectoryName = "Enemies";
        public const string PropsDirectoryName = "Props";
        public const string ItemsDirectoryName = "Items";
        public const string PrefabsDirectoryName = "TilePrefabs";
        public const string TriggersDirectoryName = "Triggers";
        
        public static string CampaignDirectoryPath => Path.Combine(PersistentPath, CampaignDirectoryName);
        private const string CampaignFileExtension = ".bytes";
        
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

        public static string GetSavePath(string mapName) => Path.Combine(CampaignDirectoryName, $"{mapName}.map");

        /// <summary>
        /// Loads last played campaign or Main Campaign if no level was played yet.
        /// </summary>
        /// <returns>Obtained campaign or null</returns>
        public static Campaign LoadLastPlayedCampaign()
        {
            string campaignName = PlayerPrefs.GetString(Strings.LastPlayedCampaign, Strings.MainCampaign);

            if (string.IsNullOrEmpty(campaignName)) return null;

            if (!File.Exists(GetFullCampaignPath(campaignName))) return null;

            try
            {
                return ES3.Load<Campaign>(campaignName, GetFullRelativeCampaignPath(campaignName));
            }
            catch (Exception e)
            {
                Logger.Log($"Failed to load campaign from file: {campaignName}: {e}", Logger.ELogSeverity.Release);
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
            EPrefabType.Trigger => TriggersDirectoryName,
            _ => throw new ArgumentOutOfRangeException(nameof(prefabType), prefabType, null)
        };

        private static string GetFullRelativeCampaignPath(string campaignName) => Path.Combine(CampaignDirectoryName, $"{campaignName}{CampaignFileExtension}");

        private static string GetFullCampaignPath(string campaignName) => Path.Combine(CampaignDirectoryPath, $"{campaignName}{CampaignFileExtension}");

        public static string GetCampaignMapKey(string campaignName, string mapName) => $"{campaignName}_{mapName}";
    }
}