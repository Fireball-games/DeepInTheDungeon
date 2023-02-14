using Scripts.Player;
using UnityEngine;

namespace Scripts.System.MonoBases
{
    public abstract class EscapeMenuBase : DialogBase
    {
        private static PlayerCameraController PlayerCameraController => PlayerCameraController.Instance;
        
        private bool _isOpened;
        private bool _isFreeLookOnOnOpen;
        
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) HandleEscapeKeyPressed();
        }
        
        protected abstract void SetContentOnShow();
        
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
            SetContentOnShow();

            if (await base.Show() is not EConfirmResult.Cancel) return;

            PlayerCameraController.IsLookModeOn = _isFreeLookOnOnOpen;
            _isOpened = false;
        }
    }
}