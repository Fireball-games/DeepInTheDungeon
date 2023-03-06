using System.Linq;
using Scripts.EventsManagement;
using Scripts.Helpers;
using Scripts.Helpers.Extensions;
using Scripts.Localization;
using Scripts.Player;
using Scripts.ScenesManagement;
using Scripts.System;
using Scripts.System.MonoBases;
using Scripts.System.Saving;
using UnityEngine;
using UnityEngine.UI;
using Logger = Scripts.Helpers.Logger;

namespace Scripts.UI
{
    public class ButtonsMenu : UIElementBase
    {
        private Button _continueCampaignButton;
        private Button _customCampaignButton;
        private Button _loadPositionButton;
        private Button _newCampaignButton;
        private Button _createNewCharacterButton;
        private Button _lastEditedMapButton;
        private Button _editorButton;
        private Button _settingsButton;
        private Button _exitGameButton;
        
        private static GameManager GameManager => GameManager.Instance;
        private MainUIManager UIManager => MainUIManager.Instance;
        
        private void Awake()
        {
            AssignComponents();
        }

        private void Start()
        {
            SetComponents();
        }
        
        private void NewCampaignClicked() => GameManager.StartMainCampaign();

        private void ContinueCampaignClicked() => GameManager.ContinueFromSave(SaveManager.CurrentSave);
        
        private void LoadPositionClicked() => UIManager.OpenLoadMenu();

        private void CustomCampaignClicked() => Logger.LogNotImplemented();

        private void LastEditedMapClicked()
        {
            OnSceneChangingButtonClicked();
            GameManager.LoadLastEditedMap();
        }

        private void CreateNewCharacterClicked() => Logger.LogNotImplemented();

        private void SettingsClicked() => Logger.LogNotImplemented();

        private void EditorClicked()
        {
            OnSceneChangingButtonClicked();
            SceneLoader.Instance.LoadEditorScene();
        }

        private void ExitGameClicked() => Application.Quit();
        
        private void OnSceneChangingButtonClicked()
        {
            body.SetActive(false);
            PlayerCameraController.Instance.IsLookModeOn = false;
        }
        
        internal void SetComponents()
        {
            _continueCampaignButton.SetText(t.Get(Keys.ContinueCampaign));
            _continueCampaignButton.interactable = SaveManager.Saves.Any();
            
            _loadPositionButton.SetText(t.Get(Keys.LoadSavedPosition));
            _loadPositionButton.interactable = SaveManager.Saves.Any();
            
            _customCampaignButton.SetText(t.Get(Keys.CustomCampaign));
            _newCampaignButton.SetText(t.Get(Keys.StartNewCampaign));
            _createNewCharacterButton.SetText(t.Get(Keys.CreateNewCharacter));
            _lastEditedMapButton.SetText(t.Get(Keys.LoadLastEditedMap));
            _editorButton.SetText(t.Get(Keys.OpenMapEditor));
            _settingsButton.SetText(t.Get(Keys.Settings));
            _exitGameButton.SetText(t.Get(Keys.ExitGame));
            
            _lastEditedMapButton.gameObject.SetActive(PlayerPrefsHelper.IsLastEditedMapValid());
        }
        
        private void AssignComponents()
        {
            Transform playButtons = body.transform.Find("Frame/PlayButtons");
            _continueCampaignButton = playButtons.Find("ContinueCampaignButton").GetComponent<Button>();
            _continueCampaignButton.onClick.AddListener(ContinueCampaignClicked);
            _loadPositionButton = playButtons.Find("LoadPositionButton").GetComponent<Button>();
            _loadPositionButton.onClick.AddListener(LoadPositionClicked);
            _customCampaignButton = playButtons.Find("CustomCampaignButton").GetComponent<Button>();
            _customCampaignButton.onClick.AddListener(CustomCampaignClicked);

            Transform systemButtons = body.transform.Find("Frame/SystemButtons");
            _newCampaignButton = systemButtons.Find("NewCampaignButton").GetComponent<Button>();
            _newCampaignButton.onClick.AddListener(NewCampaignClicked);
            _createNewCharacterButton = systemButtons.Find("CreateNewCharacterButton").GetComponent<Button>();
            _createNewCharacterButton.onClick.AddListener(CreateNewCharacterClicked);
            _settingsButton = systemButtons.Find("SettingsButton").GetComponent<Button>();
            _settingsButton.onClick.AddListener(SettingsClicked);
            _editorButton = systemButtons.Find("EditorButton").GetComponent<Button>();
            _editorButton.onClick.AddListener(EditorClicked);
            _lastEditedMapButton = systemButtons.Find("LastEditedMapButton").GetComponent<Button>();
            _lastEditedMapButton.onClick.AddListener(LastEditedMapClicked);
            _exitGameButton = systemButtons.Find("ExitGameButton").GetComponent<Button>();
            _exitGameButton.onClick.AddListener(ExitGameClicked);
        }
    }
}