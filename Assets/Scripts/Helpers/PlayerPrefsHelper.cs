using UnityEngine;

namespace Scripts.Helpers
{
    public static class PlayerPrefsHelper
    {
        private static string[] _lastEditedMap;
        public static string[] LastEditedMap
        {
            get
            {
                if (!IsLastEditedMapValid())
                {
                    _lastEditedMap = PlayerPrefs.GetString(Strings.LastEditedMap).Split('_');
                }
                
                return _lastEditedMap;
            }
            set
            {
                if (!IsCampaignMapKeyValid(value))
                    return;
                
                _lastEditedMap = value;
                PlayerPrefs.SetString(Strings.LastEditedMap, GetCampaignMapKey(value));
            }
        }
        
        private static string _lastPlayedCampaign;
        public static string LastPlayedCampaign
        {
            get
            {
                if (string.IsNullOrEmpty(_lastPlayedCampaign))
                {
                    _lastPlayedCampaign = PlayerPrefs.GetString(Strings.LastPlayedCampaign);
                }
                
                return _lastPlayedCampaign;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    return;
                
                _lastPlayedCampaign = value;
                PlayerPrefs.SetString(Strings.LastPlayedCampaign, value);
            }
        }

        public static bool IsCampaignMapKeyValid(string[] campaignMapKey)
        {
            if (campaignMapKey is not {Length: 2}) return false;
            
            if (string.IsNullOrEmpty(campaignMapKey[0])) return false;
            
            if (string.IsNullOrEmpty(campaignMapKey[1])) return false;
            
            return true;
        }

        /// <summary>
        /// Resolve if _lastEditedMap has valid values.
        /// </summary>
        public static bool IsLastEditedMapValid()
        {
            if (!IsCampaignMapKeyValid(_lastEditedMap))
            {
                _lastEditedMap = PlayerPrefs.GetString(Strings.LastEditedMap).Split('_');
            }
                
            return IsCampaignMapKeyValid(_lastEditedMap);
        }

        public static string GetCampaignMapKey(string campaignName, string mapName) => $"{campaignName}_{mapName}";
        
        public static string GetCampaignMapKey(string[] campaignMapKey) => GetCampaignMapKey(campaignMapKey[0], campaignMapKey[1]);
    }
}