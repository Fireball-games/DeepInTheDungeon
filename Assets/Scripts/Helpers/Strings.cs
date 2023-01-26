﻿using System.Collections.Generic;
using System.Linq;

namespace Scripts.Helpers
{
    public static class Strings
    {
        public const string Show = "Show";
        public const string LastEditedMap = "LastEditedMap";
        public const string LastPlayedCampaign = "LastPlayedCampaign";
        public const string MainCamera = "MainCamera";
        public const string MouseXAxis = "Mouse X";
        public const string MouseYAxis = "Mouse Y";
        public const string MouseWheel = "Mouse ScrollWheel";
        public const string Untagged = "Untagged";
        public const string MainCampaign = "DeepInTheDungeon";
        
        public static string GetDefaultName(string baseString, IEnumerable<string> existingNames)
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