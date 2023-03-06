using UnityEngine;
using static Scripts.Enums;

namespace Scripts.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Configurations/Game Configuration", fileName = "GameConfiguration")]
    public class GameConfiguration : ScriptableObject
    {
        public EMainCampaignName selectedMainCampaign = EMainCampaignName.MainCampaign;
        public int maxAutoSaves = 5;
        public int maxQuickSaves = 5;
        public int maxMapEntrySaves = 3;
        public int maxMapExitSaves = 3;
        public int maxManualSaves = 100;
    }
}