using Scripts.EventsManagement;
using Scripts.Helpers;
using Scripts.Helpers.Extensions;
using Scripts.Localization;
using Scripts.ScenesManagement;
using Scripts.System;
using UnityEngine;
using UnityEngine.UI;
using Logger = Scripts.Helpers.Logger;

namespace Scripts.UI
{
    public class MainMenu : MonoBehaviour
    {
        public GameObject body;

        private Button _newCampaignButton;
        private Button _continueCampaignButton;
        private Button _customCampaignButton;
        private Button _lastEditedMapButton;
        private Button _createNewCharacterButton;
        private Button _settingsButton;
        private Button _editorButton;
        private Button _exitGameButton;
        
        private static GameManager GameManager => GameManager.Instance;

        private void Awake()
        {
            AssignComponents();
        }

        private void Start()
        {
            SetComponents();
        }

        private void OnEnable()
        {
            EventsManager.OnLevelStarted += OnLevelStarted;
        }

        private void OnDisable()
        {
            EventsManager.OnLevelStarted -= OnLevelStarted;
        }
        
        public void RefreshButtons() => SetComponents();
        
        public void SetActive(bool active) => body.SetActive(active);
        
        private void OnLevelStarted()
        {
            SetComponents();
        }

        private void NewCampaignClicked() => Logger.LogNotImplemented();

        private void ContinueCampaignClicked() => GameManager.ContinueFromSave();

        private void CustomCampaignClicked() => Logger.LogNotImplemented();

        private void LastEditedMapClicked()
        {
            body.SetActive(false);
            GameManager.LoadLastEditedMap();
        }

        private void CreateNewCharacterClicked() => Logger.LogNotImplemented();

        private void SettingsClicked() => Logger.LogNotImplemented();

        private void EditorClicked()
        {
            body.SetActive(false);
            SceneLoader.Instance.LoadEditorScene();
        }

        private void ExitGameClicked() => Application.Quit();

        private void SetComponents()
        {
            _newCampaignButton.SetText(t.Get(Keys.StartNewCampaign));
            _continueCampaignButton.SetText(t.Get(Keys.ContinueCampaign));
            _customCampaignButton.SetText(t.Get(Keys.CustomCampaign));
            _lastEditedMapButton.SetText(t.Get(Keys.LoadLastEditedMap));
            _createNewCharacterButton.SetText(t.Get(Keys.CreateNewCharacter));
            _settingsButton.SetText(t.Get(Keys.Settings));
            _editorButton.SetText(t.Get(Keys.OpenMapEditor));
            _exitGameButton.SetText(t.Get(Keys.ExitGame));
            
            _lastEditedMapButton.gameObject.SetActive(PlayerPrefsHelper.IsLastEditedMapValid());
        }
        
        private void AssignComponents()
        {
            Transform playButtons = body.transform.Find("Frame/PlayButtons");
            _newCampaignButton = playButtons.Find("NewCampaignButton").GetComponent<Button>();
            _newCampaignButton.onClick.AddListener(NewCampaignClicked);
            _continueCampaignButton = playButtons.Find("ContinueCampaignButton").GetComponent<Button>();
            _continueCampaignButton.onClick.AddListener(ContinueCampaignClicked);
            _customCampaignButton = playButtons.Find("CustomCampaignButton").GetComponent<Button>();
            _customCampaignButton.onClick.AddListener(CustomCampaignClicked);
            _lastEditedMapButton = playButtons.Find("LastEditedMapButton").GetComponent<Button>();
            _lastEditedMapButton.onClick.AddListener(LastEditedMapClicked);
            
            Transform systemButtons = body.transform.Find("Frame/SystemButtons");
            _createNewCharacterButton = systemButtons.Find("CreateNewCharacterButton").GetComponent<Button>();
            _createNewCharacterButton.onClick.AddListener(CreateNewCharacterClicked);
            _settingsButton = systemButtons.Find("SettingsButton").GetComponent<Button>();
            _settingsButton.onClick.AddListener(SettingsClicked);
            _editorButton = systemButtons.Find("EditorButton").GetComponent<Button>();
            _editorButton.onClick.AddListener(EditorClicked);
            _exitGameButton = systemButtons.Find("ExitGameButton").GetComponent<Button>();
            _exitGameButton.onClick.AddListener(ExitGameClicked);
        }
    }
}