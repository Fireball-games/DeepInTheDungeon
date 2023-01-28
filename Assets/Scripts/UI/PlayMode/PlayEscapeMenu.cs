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
        [SerializeField] private Button toEditorButton;
        [SerializeField] private Button toMainSceneButton;

        private static PlayerCameraController PlayerCameraController => PlayerCameraController.Instance;

        private bool isOpened;
        private bool _isFreeLookOnOnOpen;

        private void Awake()
        {
            toMainSceneButton.onClick.AddListener(LeaveToMainScene);
            toEditorButton.onClick.AddListener(LeaveToEditor);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) HandleEscapeKeyPressed();
        }

        private void HandleEscapeKeyPressed()
        {
            if (!isOpened)
            {
                _isFreeLookOnOnOpen = PlayerCameraController.IsLookModeOn;
                PlayerCameraController.IsLookModeOn = false;
                PlayerCameraController.ResetCamera();
                isOpened = true;
                Show();
                return;
            }

            PlayerCameraController.IsLookModeOn = _isFreeLookOnOnOpen;
            isOpened = false;
            CloseDialog();
        }

        private async void Show()
        {
            toEditorButton.gameObject.SetActive(GameManager.Instance.IsPlayingFromEditor);
            toEditorButton.SetText(t.Get(Keys.ReturnToEditor));

            toMainSceneButton.SetText(t.Get(Keys.ReturnToMainScene));

            if (await base.Show() is not EConfirmResult.Cancel) return;

            PlayerCameraController.IsLookModeOn = _isFreeLookOnOnOpen;
            isOpened = false;
        }

        private void LeaveToMainScene() => SceneLoader.Instance.LoadMainScene();

        private void LeaveToEditor() => SceneLoader.Instance.LoadEditorScene();
    }
}