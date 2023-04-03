using System.Collections.Generic;
using System.Linq;
using Scripts.Building;
using Scripts.Helpers;

namespace Scripts.System
{
    public static class CampaignsStore
    {
        public static IEnumerable<Campaign> OfficialCampaigns { get; private set; }
        public static IEnumerable<Campaign> CustomCampaigns { get; private set; }
        public static Campaign MainCampaign { get; private set; }
        public static Campaign StartRoomsCampaign { get; private set; }

        public static bool LoadCampaigns()
        {
            OfficialCampaigns = FileOperationsHelper.LoadCampaignsFromResources();
            CustomCampaigns = FileOperationsHelper.LoadCampaignsFromPersistentDataPath();
            
            MainCampaign = OfficialCampaigns.FirstOrDefault(c => c.CampaignName == Strings.GetSelectedMainCampaignName());
            StartRoomsCampaign = OfficialCampaigns.FirstOrDefault(c => c.CampaignName == Strings.StartRoomsCampaignName);
            
            return MainCampaign != null && StartRoomsCampaign != null;
        }

        public static void AddCustomCampaign(Campaign campaign) => CustomCampaigns = CustomCampaigns.Append(campaign);
        public static void RemoveCustomCampaign(Campaign campaign) => CustomCampaigns = CustomCampaigns.Except(new[] {campaign});
    }
}