using System.Collections.Generic;
using System.Linq;
using Scripts.System;

namespace Scripts.Helpers
{
    public static class Strings
    {
        public const string DemoCampaignName = "Demo";
        public const string Show = "Show";
        public const string LastEditedMap = "LastEditedMap";
        public const string LastPlayedCampaign = "LastPlayedCampaign";
        public const string MainCamera = "MainCamera";
        public const string MainCampaignName = "DeepInTheDungeon";
        public const string MouseXAxis = "Mouse X";
        public const string MouseYAxis = "Mouse Y";
        public const string MouseWheel = "Mouse ScrollWheel";
        public const string StartRoomsCampaignName = "StartRooms";
        public const string Untagged = "Untagged";
        public const string CampaignStartMapName = "CampaignStart";

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
            string name = baseString;

            while (existingNames.Contains(name))
            {
                name = $"{baseString}{number}";
                number++;
            }

            return name;
        }
    }
}