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

        private void Awake()
        {
            _toMainSceneButton = body.transform.Find("Background/Content/ToMenuButton").GetComponent<Button>();
            _toMainSceneButton.onClick.AddListener(LeaveToMainScene);
            
            _toEditorButton = body.transform.Find("Background/Content/ToEditorButton").GetComponent<Button>();
            _toEditorButton.onClick.AddListener(LeaveToEditor);
        }

        protected override void SetContentOnShow()
        {
            _toEditorButton.gameObject.SetActive(GameManager.Instance.IsPlayingFromEditor);
            _toEditorButton.SetText(t.Get(Keys.ReturnToEditor));

            _toMainSceneButton.SetText(t.Get(Keys.ReturnToMainScene));
        }

        private void LeaveToMainScene() => SceneLoader.Instance.LoadMainScene();

        private void LeaveToEditor() => SceneLoader.Instance.LoadEditorScene();
    }
}