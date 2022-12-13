using System;
using System.Collections.Generic;
using System.IO;
using Scripts.Helpers;
using Scripts.System.MonoBases;
using Scripts.System.Pooling;
using Scripts.UI.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.UI.EditorUI
{
    public class PrefabList : EditorWindowBase
    {
        [SerializeField] private Title title;
        [SerializeField] private GameObject listContent;
        [SerializeField] private GameObject fileItemPrefab;
        
        public void Open(string listTitle, IEnumerable<string> files, Action<string> onFileItemClicked, Action onClose = null)
        {
            title.SetTitle(listTitle);

            listContent.DismissAllChildrenToPool(true);

            foreach (string file in files)  
            {
                string fileName = Path.GetFileNameWithoutExtension(file);

                GameObject fileItem = ObjectPool.Instance.GetFromPool(fileItemPrefab, listContent, true);
                fileItem.GetComponentInChildren<TMP_Text>().text = fileName;
                fileItem.GetComponentInChildren<Button>().onClick.AddListener(() => onFileItemClicked?.Invoke(file));
            }
        }
    }
}