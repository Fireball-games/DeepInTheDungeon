using System;
using System.Collections.Generic;

namespace Scripts.System.Saving
{
    [Serializable]
    public class MapSave
    {
        public string mapName;
        public IEnumerable<MapStateRecord> mapState;
    }
}