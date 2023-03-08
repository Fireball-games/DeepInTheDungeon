using Scripts.Building;
using Scripts.Helpers;
using Scripts.Helpers.Extensions;
using Scripts.Localization;
using Scripts.ScenesManagement;
using Scripts.System;
using Scripts.System.MonoBases;
using UnityEngine.UI;

namespace Scripts.UI.PlayMode
{
    public class PlayEscapeMenu : EscapeMenuBase
    {
        private Button _loadPositionButton;
        private Button _toEditorButton;
        private Button _toMainSceneButton;
        
        private LoadPositionsDialog _loadPositionsDialog; 
        
        private static GameManager GameManager => GameManager.Instance;

        private void Awake()
        {
            _loadPositionButton = body.transform.Find("Background/Content/LoadPositionButton").GetComponent<Button>();
            _loadPositionButton.onClick.AddListener(LoadPosition);
            
            _toMainSceneButton = body.transform.Find("Background/Content/ToMenuButton").GetComponent<Button>();
            _toMainSceneButton.onClick.AddListener(LeaveToMainScene);
            
            _toEditorButton = body.transform.Find("Background/Content/ToEditorButton").GetComponent<Button>();
            _toEditorButton.onClick.AddListener(LeaveToEditor);
            
            _loadPositionsDialog = FindObjectOfType<LoadPositionsDialog>(true);
        }

        protected override void SetContentOnShow()
        {
            _loadPositionButton.gameObject.SetActive(!GameManager.IsPlayingFromEditor);
            _loadPositionButton.SetText(t.Get(Keys.LoadSavedPosition));
            
            _toEditorButton.gameObject.SetActive(GameManager.IsPlayingFromEditor);
            _toEditorButton.SetText(t.Get(Keys.ReturnToEditor));

            _toMainSceneButton.SetText(t.Get(Keys.ReturnToMainScene));
            
            _loadPositionsDialog.SetActive(false);
        }
        
        private async void LoadPosition()
        {
            await _loadPositionsDialog.Show(t.Get(Keys.LoadSavedPosition), showVisibleModal: false);
        }

        private void LeaveToMainScene()
        {
            GameManager.IsPlayingFromEditor = false;
            GameManager.StartMainScene();
        }

        private void LeaveToEditor()
        {
            if (FileOperationsHelper.GetLastEditedCampaignAndMap(out Campaign campaign, out MapDescription map))
            {
                GameManager.SetCurrentCampaign(campaign);
                GameManager.SetCurrentMap(map);
            }
            else
            {
                Logger.LogWarning("Failed to get data for last edited map.");
            }
            
            SceneLoader.Instance.LoadEditorScene();
        }
    }
}