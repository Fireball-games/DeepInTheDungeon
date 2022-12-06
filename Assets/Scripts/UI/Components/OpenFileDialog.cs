using System;
using System.Collections.Generic;
using System.IO;
using Scripts.Helpers;
using Scripts.System;
using Scripts.System.Pooling;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.UI.EditorUI
{
    public class OpenFileDialog : DialogBase
    {
        [SerializeField] private GameObject fileItemsParent;
        [SerializeField] private GameObject fileItemPrefab;

        private void Awake()
        {
            cancelButton.onClick.AddListener(CloseDialog);
        }

        public void Open(string dialogTitle, IEnumerable<string> files, Action<string> onFileItemClicked, Action onClose = null)
        {
            base.Open(dialogTitle, onClose);
            
            fileItemsParent.DismissAllChildrenToPool(true);

            foreach (string file in files)  
            {
                string fileName = Path.GetFileName(file);

                GameObject fileItem = ObjectPool.Instance.GetFromPool(fileItemPrefab, fileItemsParent, true);
                fileItem.GetComponentInChildren<TMP_Text>().text = fileName;
                fileItem.GetComponentInChildren<Button>().onClick.AddListener(() => onFileItemClicked?.Invoke(file));
            }
        }
    }
}
