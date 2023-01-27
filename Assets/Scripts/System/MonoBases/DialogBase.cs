using System.Threading.Tasks;
using Scripts.EventsManagement;
using Scripts.Localization;
using Scripts.MapEditor.Services;
using Scripts.UI;
using Scripts.UI.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.System.MonoBases
{
    public class DialogBase : UIElementBase
    {
        [SerializeField] protected Title title;
        [SerializeField] protected Button cancelButton;
        [SerializeField] protected Button confirmButton;
        [SerializeField] protected TMP_Text cancelText;
        [SerializeField] protected TMP_Text confirmText;

        private TaskCompletionSource<EConfirmResult> _taskCompletionSource;
        private bool _isClosed;

        public enum EConfirmResult
        {
            Ok,
            Cancel
        }

        public async Task<EConfirmResult> Show(string dialogTitle = null, string confirmButtonText = null, string cancelButtonText = null)
        {
            _isClosed = false;
            bool confirmTextIsNull = string.IsNullOrEmpty(confirmButtonText);
            bool cancelTextIsNull = string.IsNullOrEmpty(cancelButtonText);
            
            if (!confirmTextIsNull && confirmButton) confirmText.text = confirmButtonText;
            if (!cancelTextIsNull && cancelButton) cancelText.text = cancelButtonText;
            if (title) title.SetTitle(dialogTitle);
            
            if (confirmButton)
            {
                confirmButton.onClick.RemoveAllListeners();
                confirmButton.onClick.AddListener(OnOKClicked);
                if (confirmTextIsNull) confirmText.text = t.Get(Keys.Confirm);
            }

            if (cancelButton)
            {
                cancelButton.onClick.RemoveAllListeners();
                cancelButton.onClick.AddListener(OnCancelClicked);
                if (cancelTextIsNull) cancelText.text = t.Get(Keys.Cancel);
            }

            if (GameManager.Instance.GameMode == GameManager.EGameMode.Editor)
            {
                EditorMouseService.Instance.ResetCursor();
            }
            
            EventsManager.OnModalClicked.RemoveAllListeners();
            EventsManager.OnModalClicked.AddListener(OnCancelClicked);
            Modal.Show();

            _taskCompletionSource = new TaskCompletionSource<EConfirmResult>();
            SetActive(true);
            return await _taskCompletionSource.Task;
        }

        private void OnCancelClicked()
        {
            OnConfirm(EConfirmResult.Cancel);
        }

        private void OnOKClicked()
        {
            OnConfirm(EConfirmResult.Ok);
        }
        
        public void CloseDialog()
        {
            OnConfirm(EConfirmResult.Cancel);
        }

        protected void OnConfirm(EConfirmResult result)
        {
            if (_isClosed) return;
            
            _isClosed = true;
            _taskCompletionSource.SetResult(result);
            
            Modal.Hide();
            
            SetActive(false);
        }
    }
}