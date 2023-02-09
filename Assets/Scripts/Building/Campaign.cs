using System.Collections.Generic;
using System.Linq;
using Scripts.Helpers;
using Scripts.Localization;

namespace Scripts.Building
{
    public class Campaign
    {
        public string CampaignName;
        public string StartMapName;
        public List<MapDescription> Maps;

        public Campaign()
        {
            CampaignName = t.Get(Keys.DefaultCampaignName);
            Maps = new List<MapDescription>();
        }

        public IEnumerable<string> MapsNames => Maps?.Select(map => map.MapName) ?? Enumerable.Empty<string>();

        public MapDescription GetStarterMap()
        {
            MapDescription map = Maps.FirstOrDefault(m => m.MapName == StartMapName);
            if (map != null) return map;
            
            Logger.LogWarning($"Start map not set or not found in campaign: {CampaignName}");
            return Maps[0];
        }

        /// <summary>
        /// If map already exists, then replaces that map with the new one
        /// </summary>
        /// <param name="newMapVersion"></param>
        public void ReplaceMap(MapDescription newMapVersion)
        {
            int index = Maps.FindIndex(map => map.MapName == newMapVersion.MapName);

            if (index != -1)
            {
                Maps[index] = newMapVersion;
            }
        }

        public MapDescription GetMapByName(string mapName)
        {
            MapDescription map = Maps.FirstOrDefault(m => m.MapName == mapName);

            if (map != null) return map;

            Logger.LogError($"Map with the name: {mapName} not found in campaign: {CampaignName}");
            return null;
        }

        /// <summary>
        /// Adds new map. If map already exists, then replaces that map.
        /// </summary>
        /// <param name="newMap"></param>
        public void AddReplaceMap(MapDescription newMap)
        {
            int index = Maps.FindIndex(map => map.MapName == newMap.MapName);
            if (index == -1)
            {
                Maps.Add(newMap);
            }
            else
            {
                Maps[index] = newMap;
            }
        }
    }
}