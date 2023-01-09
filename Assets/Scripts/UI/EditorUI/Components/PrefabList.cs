using System;
using System.Collections.Generic;
using Scripts.Building.Walls;
using Scripts.Helpers.Extensions;
using Scripts.System.MonoBases;
using Scripts.System.Pooling;
using Scripts.UI.Components;
using UnityEngine;

namespace Scripts.UI.EditorUI.Components
{
    public class PrefabList<T> : EditorWindowBase where T : PrefabBase 
    {
        [SerializeField] private Title title;
        [SerializeField] private GameObject listContent;
        [SerializeField] private GameObject itemPrefab;

        private HashSet<PrefabListButton<T>> _buttons;

        private Action<PrefabBase> OnItemClicked;

        private void Awake()
        {
            _buttons = new HashSet<PrefabListButton<T>>();
        }

        public void Open(string listTitle, IEnumerable<PrefabBase> prefabs, Action<PrefabBase> onItemClicked, Action onClose = null)
        {
            SetActive(true);
            title.SetTitle(listTitle);
            OnItemClicked = null;
            OnItemClicked = onItemClicked;

            listContent.gameObject.DismissAllChildrenToPool(true);

            _buttons ??= new HashSet<PrefabListButton<T>>();
            _buttons.Clear();

            if (prefabs == null) return;

            foreach (T prefab in prefabs)  
            {
                PrefabListButton<T> newButton = ObjectPool.Instance
                    .GetFromPool(itemPrefab, listContent, true)
                    .GetComponent<PrefabListButton<T>>();
                
                newButton.Set(prefab, OnItemClicked_internal);

                _buttons.Add(newButton);
            }
        }

        public void DeselectButtons() => _buttons.ForEach(b => b.SetSelected(false));

        public void Close() => SetActive(false);

        private void OnItemClicked_internal(PrefabBase prefab)
        {
            string prefabName = prefab.gameObject.name;
            
            OnItemClicked.Invoke(prefab);

            foreach (PrefabListButton<T> button in _buttons)
            {
                // TODO: get name from T type
                if (button.gameObject.name != prefabName)
                {
                    button.SetSelected(false);
                }
            }
        }
    }
}