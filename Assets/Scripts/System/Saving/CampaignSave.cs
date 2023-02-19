using System;
using System.Collections.Generic;

namespace Scripts.System.Saving
{
    [Serializable]
    public class CampaignSave
    {
        public string campaignName;
        public IEnumerable<MapSave> mapsSaves;
    }
}