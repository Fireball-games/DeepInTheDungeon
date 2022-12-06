using System;
using Scripts.EventsManagement;
using Scripts.Localization;
using Scripts.UI;
using Scripts.UI.Components;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.System
{
    public class DialogBase : UIElementBase
    {
        [SerializeField] protected TitleController title;
        [SerializeField] protected Button cancelButton;
        [SerializeField] protected Button confirmButton;
        
        protected event Action OnCancel;
        protected event Action OnOk;

        private void Awake()
        {
            cancelButton.onClick.AddListener(OnCancelClicked);
            confirmButton.onClick.AddListener(OnOKClicked);
        }

        public void Open(string dialogTitle, Action onOk = null, Action onCancel = null)
        {
            title.SetTitle(dialogTitle);
            OnCancel = onCancel;
            OnOk = onOk;
            EventsManager.OnModalClicked += CloseDialog;
            Modal.Show();

            SetActive(true);
        }

        protected void CloseDialog()
        {
            EventsManager.OnModalClicked -= CloseDialog;
            Modal.Hide();
            
            SetActive(false);
        }

        protected void OnCancelClicked()
        {
            OnCancel?.Invoke();
            OnCancel = null;
            
            CloseDialog();
        }

        protected void OnOKClicked()
        {
            OnOk?.Invoke();
            OnOk = null;
            
            CloseDialog();
        }
    }
}