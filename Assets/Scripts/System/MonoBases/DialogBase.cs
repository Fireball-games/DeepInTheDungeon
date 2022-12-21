using System;
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
        
        protected event Action OnCancel;
        protected event Action OnOk;

        private void Awake()
        {
            if (cancelButton)
            {
                cancelButton.onClick.AddListener(OnCancelClicked);
                cancelText.text = t.Get(Keys.Cancel);
            }

            if (confirmButton)
            {
                confirmButton.onClick.AddListener(OnOKClicked);
                confirmText.text = t.Get(Keys.Confirm);
            }
        }

        public void Open(
            string dialogTitle = null,
            Action onOk = null,
            Action onCancel = null,
            string confirmButtonText = null,
            string cancelButtonText = null)
        {
            if (!string.IsNullOrEmpty(confirmButtonText) && confirmButton) confirmText.text = confirmButtonText;
            if (!string.IsNullOrEmpty(cancelButtonText) && cancelButton) cancelText.text = cancelButtonText;
            if (title) title.SetTitle(dialogTitle);

            if (GameManager.Instance.GameMode == GameManager.EGameMode.Editor)
            {
                EditorMouseService.Instance.ResetCursor();
            }
            
            OnCancel = onCancel;
            OnOk = onOk;
            EventsManager.OnModalClicked += CloseDialog;
            Modal.Show();

            SetActive(true);
        }

        public void CloseDialog()
        {
            EventsManager.OnModalClicked -= CloseDialog;
            Modal.Hide();
            
            SetActive(false);
        }

        private void OnCancelClicked()
        {
            OnCancel?.Invoke();
            OnCancel = null;
            
            CloseDialog();
        }

        private void OnOKClicked()
        {
            OnOk?.Invoke();
            OnOk = null;
            
            CloseDialog();
        }
    }
}