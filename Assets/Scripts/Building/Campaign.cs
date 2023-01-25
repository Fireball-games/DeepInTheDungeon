using System.Collections.Generic;
using System.Linq;
using Scripts.Helpers;
using Scripts.Localization;
using NotImplementedException = System.NotImplementedException;

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

        public MapDescription GetMapByName(string mapName)
        {
            MapDescription map = Maps.FirstOrDefault(m => m.MapName == mapName);
            
            if (map != null) return map;
            
            Logger.LogError($"Map with the name: {mapName} not found in campaign: {CampaignName}");
            return null;

        }
    }
}