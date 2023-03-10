using System.Threading.Tasks;
using Scripts.Helpers;
using Scripts.Helpers.Extensions;
using Scripts.Localization;
using Scripts.Player.CharacterSystem;
using Scripts.System;
using Scripts.System.MonoBases;
using Scripts.System.Saving;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Scripts.UI.MainMenus
{
    public class PositionRecord : UIElementBase
    {
        private Button _button;
        private RawImage _screenShot;
        private TMP_Text _positionName;
        private TMP_Text _dateTime;
        private TMP_Text _campaignName;
        private TMP_Text _mapName;
        private TMP_Text _playerInfo;

        private void Awake()
        {
            AssignComponents();
        }
        
        public async Task Set(Save save, UnityAction onClick)
        {
            SetActive(false);
            PrepareForTransition();
            GetComponent<RectTransform>().FixScrollViewPosition();

            if (!_button) AssignComponents();
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(onClick);
            
            _screenShot.texture = ScreenShotService.Instance.GetScreenshotTextureFromBytes(save.screenshot);
            _positionName.text = save.saveName;
            _dateTime.text = save.timeStamp.ToString("dd-MMMM-yy H:mm");
            _campaignName.text = $"{t.Get(Keys.Campaign).WrapInColor(Colors.Positive)}: {save.CurrentCampaign}";
            _mapName.text = $"{t.Get(Keys.Map).WrapInColor(Colors.Positive)}: {save.CurrentMap}";;
            _playerInfo.text = BuildPlayerInfoString(save.characterProfile);
            await SetActiveAsync(true);
        }

        private string BuildPlayerInfoString(CharacterProfile saveCharacterProfile)
        {
            // TODO: Get info from character profile
            return "Not implemented yet";
        }

        private void AssignComponents()
        {
            _screenShot = body.transform.Find("ScreenShot").GetComponent<RawImage>();
            
            _button = GetComponent<Button>();
            
            Transform nameTime = body.transform.Find("Info/NameTime");
            _positionName = nameTime.Find("PositionName").GetComponent<TMP_Text>();
            _dateTime = nameTime.Find("DateTime").GetComponent<TMP_Text>();
            
            Transform campaignMap = body.transform.Find("Info/Location");
            _campaignName = campaignMap.Find("Campaign").GetComponent<TMP_Text>();
            _mapName = campaignMap.Find("Map").GetComponent<TMP_Text>();
            
            Transform playerInfo = body.transform.Find("Info/PlayerInfo");
            _playerInfo = playerInfo.Find("PlayerInfo").GetComponent<TMP_Text>();
        }
    }
}