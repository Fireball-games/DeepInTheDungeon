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

        private void Awake()
        {
            AssignComponents();
        }

        public override async Task SetActiveAsync(bool isActive)
        {
            if (isActive) _titleText.text = t.Get(Keys.SelectCampaignToStart);
            
            await base.SetActiveAsync(isActive);
            
        }
        
        private void AssignComponents()
        {
            _titleText = transform.Find("Background/Heading/LabelFrame/Title").GetComponent<TMP_Text>();
        }
    }
}