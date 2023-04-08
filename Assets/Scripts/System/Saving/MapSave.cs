using System;
using System.Collections.Generic;
using Scripts.Building.ItemSpawning;

namespace Scripts.System.Saving
{
    [Serializable]
    public class MapSave
    {
        public string mapName;
        public List<MapStateRecord> mapState;
        public List<MapObjectConfiguration> mapObjects;
    }
}