using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Scripts.Building;
using Scripts.Helpers;
using Scripts.Helpers.Extensions;
using Scripts.Localization;
using Scripts.System;
using Scripts.System.MonoBases;
using Scripts.System.Pooling;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.UI.MainMenus
{
    public class StartCampaignMenu : UIElementBase
    {
        [SerializeField] private Button buttonPrefab;
        
        private TMP_Text _titleText;
        private TMP_Text _officialCampaignsText;
        private Transform _officialCampaignsParent;
        private TMP_Text _customCampaignsText;
        private Transform _customCampaignsParent;

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

            SetList(CampaignsStore.OfficialCampaigns.Where(c => c.CampaignName != Strings.StartRoomsCampaignName), _officialCampaignsParent);
            SetList(CampaignsStore.CustomCampaigns, _customCampaignsParent);
            await base.SetActiveAsync(isActive);
        }
        
        private void SetList(IEnumerable<Campaign> campaigns, Transform buttonParent)
        {
            buttonParent.gameObject.DismissAllChildrenToPool();

            foreach (Campaign campaign in campaigns)
            {
                Button button = ObjectPool.Instance.Get(buttonPrefab.gameObject, buttonParent.gameObject).GetComponent<Button>();
                button.GetComponent<RectTransform>().FixScrollViewPosition();
                
                button.SetText(campaign.CampaignName.FromCamelCase());
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => GameManager.Instance.StartCampaign(campaign));
            }
        }

        private void AssignComponents()
        {
            _titleText = transform.Find("Background/Heading/LabelFrame/Title").GetComponent<TMP_Text>();
            _officialCampaignsText = transform.Find("Background/Frame/OfficialCampaigns/Label/Text").GetComponent<TMP_Text>();
            _officialCampaignsParent = transform.Find("Background/Frame/OfficialCampaigns/ScrollView/Viewport/Content");
            _customCampaignsText = transform.Find("Background/Frame/CustomCampaigns/Label/Text").GetComponent<TMP_Text>();
            _customCampaignsParent = transform.Find("Background/Frame/CustomCampaigns/ScrollView/Viewport/Content");
        }
    }
}