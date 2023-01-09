using System;
using System.Collections.Generic;
using Scripts.Helpers.Extensions;
using Scripts.System.MonoBases;
using Scripts.System.Pooling;
using Scripts.UI.Components;
using UnityEngine;

namespace Scripts.UI.EditorUI.Components
{
    public abstract class ListWindowBase<T, TButton> : EditorWindowBase where TButton : ListButtonBase<T>
    {
        [SerializeField] private Title title;
        [SerializeField] private GameObject listContent;
        [SerializeField] private GameObject itemPrefab;

        protected HashSet<TButton> _buttons;

        protected Action<T> OnItemClicked;

        private void Awake()
        {
            _buttons = new HashSet<TButton>();
        }

        public void Open(string listTitle, IEnumerable<T> prefabs, Action<T> onItemClicked, Action onClose = null)
        {
            SetActive(true);
            title.SetTitle(listTitle);
            OnItemClicked = null;
            OnItemClicked = onItemClicked;

            listContent.gameObject.DismissAllChildrenToPool(true);

            _buttons ??= new HashSet<TButton>();
            _buttons.Clear();

            if (prefabs == null) return;

            foreach (T prefab in prefabs)  
            {
                TButton newButton = ObjectPool.Instance
                    .GetFromPool(itemPrefab, listContent, true)
                    .GetComponent<TButton>();
                
                newButton.Set(prefab, OnItemClicked_internal);

                _buttons.Add(newButton);
            }
        }

        public void DeselectButtons() => _buttons.ForEach(b => b.SetSelected(false));

        public void Close() => SetActive(false);

        protected abstract void OnItemClicked_internal(T item);
    }
}