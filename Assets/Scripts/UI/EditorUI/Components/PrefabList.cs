using System;
using System.Collections.Generic;
using Scripts.Building.Walls;
using Scripts.Helpers.Extensions;
using Scripts.System.MonoBases;
using Scripts.System.Pooling;
using Scripts.UI.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.UI.EditorUI.Components
{
    public class PrefabList : EditorWindowBase
    {
        [SerializeField] private Title title;
        [SerializeField] private GameObject listContent;
        [SerializeField] private GameObject itemPrefab;

        private HashSet<PrefabListButton> _buttons;

        private Action<string> OnItemClicked;

        private void Awake()
        {
            _buttons = new HashSet<PrefabListButton>();
        }

        public void Open(string listTitle, IEnumerable<PrefabBase> prefabs, Action<string> onItemClicked, Action onClose = null)
        {
            SetActive(true);
            title.SetTitle(listTitle);
            OnItemClicked = null;
            OnItemClicked = onItemClicked;

            listContent.gameObject.DismissAllChildrenToPool(true);

            _buttons ??= new HashSet<PrefabListButton>();
            _buttons.Clear();

            if (prefabs == null) return;

            foreach (PrefabBase prefab in prefabs)  
            {
                PrefabListButton newButton = ObjectPool.Instance
                    .GetFromPool(itemPrefab, listContent, true)
                    .GetComponent<PrefabListButton>();
                
                newButton.Set(prefab, OnItemClicked_internal);

                _buttons.Add(newButton);
            }
        }

        public void DeselectButtons() => _buttons.ForEach(b => b.SetSelected(false));

        public void Close() => SetActive(false);

        private void OnItemClicked_internal(PrefabBase prefab)
        {
            string prefabName = prefab.gameObject.name;
            
            OnItemClicked.Invoke(prefabName);

            foreach (PrefabListButton button in _buttons)
            {
                if (button.displayedPrefab.gameObject.name != prefabName)
                {
                    button.SetSelected(false);
                }
            }
        }
    }
}