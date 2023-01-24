using System.Collections.Generic;
using System.Linq;
using Scripts.Localization;

namespace Scripts.Building
{
    public class Campaign
    {
        public string CampaignName;
        public string StartMapName;
        public string LastPlayedMap;
        public List<MapDescription> Maps;

        public Campaign()
        {
            CampaignName = t.Get(Keys.DefaultCampaignName);
            Maps = new List<MapDescription>();
        }

        public IEnumerable<string> MapsNames => Maps?.Select(map => map.MapName) ?? Enumerable.Empty<string>();

        public MapDescription GetStarterMap()
        {
            return !string.IsNullOrEmpty(LastPlayedMap) 
                ? Maps.FirstOrDefault(map => map.MapName == LastPlayedMap) 
                : Maps.FirstOrDefault(map => map.MapName == StartMapName);
        }

        public void ReplaceMap(MapDescription newMapVersion)
        {
            int index = Maps.FindIndex(map => map.MapName == newMapVersion.MapName);
            Maps[index] = newMapVersion;
        }
    }
}