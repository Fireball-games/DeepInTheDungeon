using System.Collections.Generic;
using System.Linq;
using Scripts.Localization;
using NotImplementedException = System.NotImplementedException;

namespace Scripts.Building
{
    public class Campaign
    {
        public string CampaignID;
        public string StartMapName;
        public string LastPlayedMap;
        public List<MapDescription> Maps;

        public Campaign()
        {
            CampaignID = t.Get(Keys.DefaultCampaignName);
            Maps = new List<MapDescription>();
        }

        public MapDescription GetStartMap()
        {
            if (!string.IsNullOrEmpty(LastPlayedMap))
            {
                return Maps.FirstOrDefault(map => map.MapName == LastPlayedMap);
            }
            
            return Maps.FirstOrDefault(map => map.MapName == StartMapName);
        }
    }
}