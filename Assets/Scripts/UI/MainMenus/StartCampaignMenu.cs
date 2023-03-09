using System;
using System.Threading.Tasks;
using Scripts.Localization;
using Scripts.System.MonoBases;
using TMPro;

namespace Scripts.UI
{
    public class StartCampaignMenu : UIElementBase
    {
        private TMP_Text _titleText;
        private TMP_Text _officialCampaignsText;
        private TMP_Text _customCampaignsText;

        private void Awake()
        {
            AssignComponents();
        }

        public override async Task SetActiveAsync(bool isActive)
        {
            if (isActive)
            {
                _titleText.text = t.Get(Keys.SelectCampaignToStart);
                _officialCampaignsText.text = t.Get(Keys.OfficialCampaigns);
                _customCampaignsText.text = t.Get(Keys.CustomCampaigns);
            }
            
            await base.SetActiveAsync(isActive);
            
        }
        
        private void AssignComponents()
        {
            _titleText = transform.Find("Background/Heading/LabelFrame/Title").GetComponent<TMP_Text>();
            _officialCampaignsText = transform.Find("Background/Frame/OfficialCampaigns/Label/Text").GetComponent<TMP_Text>();
            _customCampaignsText = transform.Find("Background/Frame/CustomCampaigns/Label/Text").GetComponent<TMP_Text>();
        }
    }
}