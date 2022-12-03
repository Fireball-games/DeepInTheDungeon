using System;
using System.Collections.Generic;
using System.IO;
using Scripts.EventsManagement;
using Scripts.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.UI.EditorUI
{
    public class OpenFileDialogController : MonoBehaviour
    {
        [SerializeField] private GameObject body;
        [SerializeField] private TMP_Text title;
        [SerializeField] private Button cancelButton;
        [SerializeField] private GameObject fileItemsParent;
        [SerializeField] private GameObject fileItemPrefab;

        private event Action OnClose;

        private void Awake()
        {
            cancelButton.onClick.AddListener(CloseDialog);
        }

        public void Open(string dialogTitle, IEnumerable<string> files, Action<string> onFileItemClicked, Action onClose = null)
        {
            title.text = dialogTitle;
            OnClose = onClose;
            EventsManager.OnModalClicked += CloseDialog;
            EventsManager.TriggerOnModalShowRequested();
        
            fileItemsParent.DestroyAllChildren();

            foreach (string file in files)  
            {
                string fileName = Path.GetFileName(file);

                GameObject fileItem = Instantiate(fileItemPrefab, fileItemsParent.transform);
                fileItem.GetComponentInChildren<TMP_Text>().text = fileName;
                fileItem.GetComponentInChildren<Button>().onClick.AddListener(() => onFileItemClicked?.Invoke(file));
            }
        
            body.SetActive(true);
        }

        private void CloseDialog()
        {
            EventsManager.OnModalClicked -= CloseDialog;
            body.SetActive(false);
            OnClose?.Invoke();
            EventsManager.TriggerOnModalHideRequested();
        }
    }
}
