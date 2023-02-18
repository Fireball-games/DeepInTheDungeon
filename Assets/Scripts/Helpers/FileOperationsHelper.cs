using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Scripts.Building;
using Scripts.UI;
using UnityEngine;
using static Scripts.Enums;

namespace Scripts.Helpers
{
    public static class FileOperationsHelper
    {
        public const string CampaignDirectoryName = "Campaigns";
        public const string WallsDirectoryName = "Walls";
        public const string EnemiesDirectoryName = "Enemies";
        public const string PropsDirectoryName = "Props";
        public const string ItemsDirectoryName = "Items";
        public const string PrefabsDirectoryName = "TilePrefabs";
        public const string TriggersDirectoryName = "Triggers";
        public const string ServicesDirectoryName = "ServicePrefabs";
        
        public static string CampaignsLocalDirectoryPath => Path.Combine(PersistentPath, CampaignDirectoryName);
        public static string ApplicationResourcesPath => Path.Combine(Application.dataPath, "Resources");
        public static string FullCampaignsResourcesPath => Path.Combine(ApplicationResourcesPath, CampaignDirectoryName);
        public const string CampaignFileExtension = ".bytes";

        private static readonly string PersistentPath = Application.persistentDataPath;
        
        private static readonly ES3Settings ES3ResourcesLocationSettings;
        
        static FileOperationsHelper()
        {
            ES3ResourcesLocationSettings = new ES3Settings
            {
                location = ES3.Location.Resources,
            };
        }

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
        
        public static string GetFullCampaignPath(string campaignName) => Path.Combine(CampaignsLocalDirectoryPath, $"{campaignName}{CampaignFileExtension}");

        public static string GetSavePath(string campaignName) => Path.Combine(CampaignDirectoryName, $"{campaignName}{CampaignFileExtension}");

        public static void SaveCampaign(Campaign campaign, Action onSaveFailed = null)
        {
            if (campaign == null) return;

            try
            {
                ES3.Save(campaign.CampaignName, campaign, GetFullRelativeCampaignPath(campaign.CampaignName));
                PlayerPrefs.SetString(Strings.LastPlayedCampaign, campaign.CampaignName);
            }
            catch (Exception e)
            {
                Logger.Log($"Failed to save campaign: {campaign.CampaignName}: {e}", Logger.ELogSeverity.Release);
                onSaveFailed?.Invoke();
            }
        }
        
        public static Campaign LoadStartRoomsCampaign()
        {
            return LoadCampaign(Strings.StartRoomsCampaignName, out Campaign campaign) ? campaign : null;
        }

        /// <summary>
        /// Loads last played campaign.
        /// </summary>
        /// <returns>Obtained campaign or null</returns>
        public static Campaign LoadLastPlayedCampaign()
        {
            string campaignName = PlayerPrefsHelper.LastPlayedCampaign;

            return LoadCampaign(campaignName, out Campaign campaign) ? campaign : null;
        }
        
        public static bool GetLastEditedCampaignAndMap(out Campaign campaign, out MapDescription map)
        {
            campaign = null;
            map = null;

            campaign = LoadLastEditedCampaign();
            
            if (campaign != null && campaign.HasMapWithName(PlayerPrefsHelper.LastEditedMap[1]))
            {
                map = campaign.GetMapByName(PlayerPrefsHelper.LastEditedMap[1]);
            }
            else
            {
                PlayerPrefsHelper.RemoveLastEditedMap();
                
                if (MainUIManager.Instance)
                {
                    MainUIManager.Instance.RefreshMainMenuButtons();
                }
                
                Logger.LogError("Could not load last edited map.");
            }
            
            return map != null;
        }
        
        public static Campaign LoadLastEditedCampaign()
        {
            string[] campaignMapKey = PlayerPrefsHelper.LastEditedMap;

            if (!PlayerPrefsHelper.IsCampaignMapKeyValid(campaignMapKey)) return null;

            return LoadCampaign(campaignMapKey[0], out Campaign campaign) ? campaign : null;
        }

        public static bool LoadPrefabs(EPrefabType prefabType, out HashSet<GameObject> loadedPrefabs)
        {
            loadedPrefabs = Resources.LoadAll<GameObject>(GetPrefabPathByType(prefabType)).ToHashSet();

            return loadedPrefabs != null && loadedPrefabs.Any();
        }
        
        public static bool LoadCampaign(string campaignName, out Campaign campaign)
        {
            campaign = null;

            if (string.IsNullOrEmpty(campaignName)) return false;

            if (!File.Exists(GetFullCampaignPath(campaignName))) return false;

            try
            {
                campaign = ES3.Load<Campaign>(campaignName, GetFullRelativeCampaignPath(campaignName));
                return true;
            }
            catch (Exception e)
            {
                Logger.Log($"Failed to load campaign from file: {campaignName}: {e}", Logger.ELogSeverity.Release);
                return false;
            }
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
            EPrefabType.TriggerOnWall => TriggersDirectoryName,
            EPrefabType.TriggerTile => TriggersDirectoryName,
            EPrefabType.Service => ServicesDirectoryName,
            _ => throw new ArgumentOutOfRangeException(nameof(prefabType), prefabType, null)
        };

        private static string GetFullRelativeCampaignPath(string campaignName) => Path.Combine(CampaignDirectoryName, $"{campaignName}{CampaignFileExtension}");

        public static bool LoadSystemCampaigns(out Campaign mainCampaign, out Campaign startRoomsCampaign)
        {
            mainCampaign = null;
            startRoomsCampaign = null;

            if (!LoadResourcesCampaign(Strings.GetSelectedMainCampaignName(), out mainCampaign))
            {
                Logger.LogError($"Failed to load main campaign: {Strings.MainCampaignName}");
                return false;
            }

            if (!LoadResourcesCampaign(Strings.StartRoomsCampaignName, out startRoomsCampaign))
            {
                Logger.LogError($"Failed to load start rooms campaign: {Strings.StartRoomsCampaignName}");
                return false;
            }

            return true;
        }

        // Loads campaign from resources folder using ES3
        private static bool LoadResourcesCampaign(string campaignName, out Campaign campaign)
        {
            campaign = null;

            if (string.IsNullOrEmpty(campaignName)) return false;

            try
            {
                campaign = ES3.Load<Campaign>(campaignName, Path.Combine(CampaignDirectoryName, $"{campaignName}{CampaignFileExtension}"), ES3ResourcesLocationSettings);
                return true;
            }
            catch (Exception e)
            {
                Logger.Log($"Failed to load campaign from resources: {campaignName}: {e}", Logger.ELogSeverity.Release);
                return false;
            }
        }
    }
}