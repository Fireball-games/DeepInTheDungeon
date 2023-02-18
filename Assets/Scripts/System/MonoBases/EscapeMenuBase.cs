using Scripts.Player;
using UnityEngine;

namespace Scripts.System.MonoBases
{
    public abstract class EscapeMenuBase : DialogBase
    {
        protected bool VisibleModal = true;
        protected bool CancelOnModalClick = true;
        protected bool CancelOnEscape = true;
        
        private static PlayerCameraController PlayerCameraController => PlayerCameraController.Instance;
        
        private bool _isOpened;
        private bool _isFreeLookOnOnOpen;
        
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) HandleEscapeKeyPressed();
        }

        private void HandleEscapeKeyPressed()
        {
            if (_isOpened && !CancelOnEscape) return;
            
            if (!_isOpened)
            {
                _isFreeLookOnOnOpen = PlayerCameraController.IsLookModeOn;
                PlayerCameraController.IsLookModeOn = false;
                PlayerCameraController.ResetCamera();
                _isOpened = true;
                Show(VisibleModal);
                return;
            }

            PlayerCameraController.IsLookModeOn = _isFreeLookOnOnOpen;
            _isOpened = false;
            SetContentOnClose();
            CloseDialog();
        }
        
        private async void Show(bool visibleModal)
        {
            SetContentOnShow();

            if (await base.Show(isModalClosingDialog: CancelOnModalClick, showVisibleModal: visibleModal) is not EConfirmResult.Cancel) return;

            PlayerCameraController.IsLookModeOn = _isFreeLookOnOnOpen;
            _isOpened = false;
        }
    }
}