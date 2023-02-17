using UnityEngine;
using static Scripts.Enums;

namespace Scripts.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Configurations/Game Configuration", fileName = "GameConfiguration")]
    public class GameConfiguration : ScriptableObject
    {
        public EMainCampaignName selectedMainCampaign = EMainCampaignName.MainCampaign;
    }
}