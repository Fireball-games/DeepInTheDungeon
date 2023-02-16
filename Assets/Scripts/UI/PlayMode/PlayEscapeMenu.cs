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
        private Button _toEditorButton;
        private Button _toMainSceneButton;
        
        private static GameManager GameManager => GameManager.Instance;

        private void Awake()
        {
            _toMainSceneButton = body.transform.Find("Background/Content/ToMenuButton").GetComponent<Button>();
            _toMainSceneButton.onClick.AddListener(LeaveToMainScene);
            
            _toEditorButton = body.transform.Find("Background/Content/ToEditorButton").GetComponent<Button>();
            _toEditorButton.onClick.AddListener(LeaveToEditor);
        }

        protected override void SetContentOnShow()
        {
            _toEditorButton.gameObject.SetActive(GameManager.IsPlayingFromEditor);
            _toEditorButton.SetText(t.Get(Keys.ReturnToEditor));

            _toMainSceneButton.SetText(t.Get(Keys.ReturnToMainScene));
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