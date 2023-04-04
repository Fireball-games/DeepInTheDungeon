using System.Collections.Generic;
using System.Linq;
using Scripts.System;

namespace Scripts.Helpers
{
    public static class Strings
    {
        // Common
        public const string CampaignStartMapName = "CampaignStart";
        public const string LastEditedMap = "LastEditedMap";
        public const string LastPlayedCampaign = "LastPlayedCampaign";
        public const string MainCamera = "MainCamera";
        public const string MouseXAxis = "Mouse X";
        public const string MouseYAxis = "Mouse Y";
        public const string MouseWheel = "Mouse ScrollWheel";
        public const string Screenshot = "Screenshot";
        public const string Show = "Show";
        public const string Untagged = "Untagged";
        // FileOperations
        public const string DemoCampaignName = "Demo";
        public const string CampaignDirectoryName = "Campaigns";
        public const string CampaignFileExtension = ".bytes";
        public const string EnemiesDirectoryName = "Enemies";
        public const string ItemsDirectoryName = "Items";
        public const string MainCampaignName = "DeepInTheDungeon";
        public const string PrefabsDirectoryName = "TilePrefabs";
        public const string PropsDirectoryName = "Props";
        public const string ResourcesDirectoryName = "Resources";
        public const string SavesDirectoryName = "Save";
        public const string SaveFileExtension = ".sav";
        public const string ServicesDirectoryName = "ServicePrefabs";
        public const string StartRoomsCampaignName = "StartRooms";
        public const string TriggersDirectoryName = "Triggers";
        public const string WallsDirectoryName = "Walls";

        public static string GetSelectedMainCampaignName()
        {
            if (!GameManager.Instance) return null;
            
            return GameManager.Instance.GameConfiguration.selectedMainCampaign == Enums.EMainCampaignName.MainCampaign
                ? MainCampaignName
                : DemoCampaignName;
        }
        
        public static string IncrementName(this string baseString, IEnumerable<string> existingNames)
        {
            int number = 1;
            string name = $"{baseString}{number}";

            while (existingNames.Contains(name))
            {
                number++;
                name = $"{baseString}{number}";
            }

            return name;
        }

        public static string FromCamelCase(this string camelCaseString) =>
            string.Concat(camelCaseString.Select((x, i) => i > 0 && char.IsUpper(x) ? " " + x : x.ToString()));
        
        public static string PrivateToPublicName(this string privateName) =>
            privateName.Replace("_", "").First().ToString().ToUpper() + privateName.Replace("_", "")[1..];
    }
}