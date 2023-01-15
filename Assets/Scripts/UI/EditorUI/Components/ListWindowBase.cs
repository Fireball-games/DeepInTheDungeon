using System.Collections.Generic;
using Scripts.Helpers.Extensions;
using Scripts.System.MonoBases;
using Scripts.System.Pooling;
using Scripts.UI.Components;
using UnityEngine;
using UnityEngine.Events;

namespace Scripts.UI.EditorUI.Components
{
    public abstract class ListWindowBase<T, TButton> : EditorWindowBase where TButton : ListButtonBase<T>
    {
        [SerializeField] private Title title;
        [SerializeField] private GameObject listContent;
        [SerializeField] private GameObject itemPrefab;
        public bool SetClickedItemSelected = true;

        private HashSet<TButton> Buttons;
        private UnityEvent<T> OnItemClicked { get; } = new();
        private UnityEvent OnCancelClicked { get; } = new();

        public void Open(string listTitle, IEnumerable<T> items, UnityAction<T> onItemClicked, UnityAction onClose = null)
        {
            SetActive(true);
            title.SetTitle(listTitle);
            OnItemClicked.RemoveAllListeners();
            OnItemClicked.AddListener(onItemClicked);
            OnCancelClicked.RemoveAllListeners();
            
            if (onClose != null)
            {
                OnCancelClicked.AddListener(onClose);
            }

            listContent.gameObject.DismissAllChildrenToPool(true);

            Buttons ??= new HashSet<TButton>();
            Buttons.Clear();

            if (items == null) return;

            foreach (T prefab in items)  
            {
                TButton newButton = ObjectPool.Instance
                    .GetFromPool(itemPrefab, listContent)
                    .GetComponent<TButton>();
                
                newButton.Set(prefab, OnItemClicked_internal, SetClickedItemSelected);

                Buttons.Add(newButton);
            }
        }

        public void DeselectButtons() => Buttons.ForEach(b => b.SetSelected(false));

        public void Close() => SetActive(false);

        protected abstract string GetItemIdentification(T item);

        protected virtual void OnItemClicked_internal(T item)
        {
            if (SetClickedItemSelected)
            {
                string prefabName = GetItemIdentification(item);
            
                foreach (TButton button in Buttons)
                {
                    if (GetItemIdentification(button.displayedItem) != prefabName)
                    {
                        button.SetSelected(false);
                    }
                }
            }
            
            OnItemClicked.Invoke(item);
        }

        protected virtual void OnCancelClicked_internal()
        {
            SetActive(false);
        }
    }
}