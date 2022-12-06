using System;
using Scripts.EventsManagement;
using Scripts.UI.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.System
{
    public class DialogBase : UIElementBase
    {
        [SerializeField] protected TitleController title;
        [SerializeField] protected Button cancelButton;
        
        protected event Action OnClose;

        private void Awake()
        {
            cancelButton.onClick.AddListener(CloseDialog);
        }

        public void Open(string dialogTitle, Action onClose = null)
        {
            title.SetTitle(dialogTitle);
            OnClose = onClose;
            EventsManager.OnModalClicked += CloseDialog;
            EventsManager.TriggerOnModalShowRequested();

            SetActive(true);
        }

        protected void CloseDialog()
        {
            EventsManager.OnModalClicked -= CloseDialog;
            OnClose?.Invoke();
            OnClose = null;
            EventsManager.TriggerOnModalHideRequested();
            
            SetActive(false);
        }
    }
}