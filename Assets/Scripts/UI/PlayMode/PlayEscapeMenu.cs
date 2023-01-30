using Scripts.Helpers.Extensions;
using Scripts.Localization;
using Scripts.ScenesManagement;
using Scripts.System;
using Scripts.System.MonoBases;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.UI.PlayMode
{
    public class PlayEscapeMenu : DialogBase
    {
        private Button _toEditorButton;
        private Button _toMainSceneButton;

        private static PlayerCameraController PlayerCameraController => PlayerCameraController.Instance;

        private bool _isOpened;
        private bool _isFreeLookOnOnOpen;

        private void Awake()
        {
            _toMainSceneButton = body.transform.Find("Background/Content/ToMenuButton").GetComponent<Button>();
            _toMainSceneButton.onClick.AddListener(LeaveToMainScene);
            
            _toEditorButton = body.transform.Find("Background/Content/ToEditorButton").GetComponent<Button>();
            _toEditorButton.onClick.AddListener(LeaveToEditor);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) HandleEscapeKeyPressed();
        }

        private void HandleEscapeKeyPressed()
        {
            if (!_isOpened)
            {
                _isFreeLookOnOnOpen = PlayerCameraController.IsLookModeOn;
                PlayerCameraController.IsLookModeOn = false;
                PlayerCameraController.ResetCamera();
                _isOpened = true;
                Show();
                return;
            }

            PlayerCameraController.IsLookModeOn = _isFreeLookOnOnOpen;
            _isOpened = false;
            CloseDialog();
        }

        private async void Show()
        {
            _toEditorButton.gameObject.SetActive(GameManager.Instance.IsPlayingFromEditor);
            _toEditorButton.SetText(t.Get(Keys.ReturnToEditor));

            _toMainSceneButton.SetText(t.Get(Keys.ReturnToMainScene));

            if (await base.Show() is not EConfirmResult.Cancel) return;

            PlayerCameraController.IsLookModeOn = _isFreeLookOnOnOpen;
            _isOpened = false;
        }

        private void LeaveToMainScene() => SceneLoader.Instance.LoadMainScene();

        private void LeaveToEditor() => SceneLoader.Instance.LoadEditorScene();
    }
}