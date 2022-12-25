using System;
using System.Collections.Generic;
using System.IO;
using Scripts.System.MonoBases;
using Scripts.System.Pooling;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.UI.Components
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
            
            // fileItemsParent.DismissAllChildrenToPool(true);

            foreach (Button button in fileItemsParent.GetComponentsInChildren<Button>())
            {
                button.onClick.RemoveAllListeners();
                ObjectPool.Instance.ReturnToPool(button.transform.parent.gameObject, true);
            }

            foreach (string file in files)  
            {
                string fileName = Path.GetFileNameWithoutExtension(file);

                GameObject fileItem = ObjectPool.Instance.GetFromPool(fileItemPrefab, fileItemsParent, true);
                fileItem.GetComponentInChildren<TMP_Text>().text = fileName;
                fileItem.GetComponentInChildren<Button>().onClick.AddListener(() => onFileItemClicked?.Invoke(file));
            }
        }
    }
}
