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
        
        protected TButton LastAddedButton; 

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

            listContent.gameObject.DismissAllChildrenToPool();

            Buttons ??= new HashSet<TButton>();
            Buttons.Clear();

            if (items == null) return;

            foreach (T item in items)  
            {
                TButton newButton = ObjectPool.Instance.Get(itemPrefab, listContent).GetComponent<TButton>();

                SetButton(newButton, item);

                Buttons.Add(newButton);
                LastAddedButton = newButton;
            }
        }

        /// <summary>
        /// Button set is separated from Open method to allow for custom button settings.
        /// </summary>
        /// <param name="button"></param>
        /// <param name="item"></param>
        protected virtual void SetButton(TButton button, T item)
        {
            button.Set(item, OnItemClicked_internal, SetClickedItemSelected);
        }

        public void DeselectButtons() => Buttons?.ForEach(b => b.SetSelected(false));

        public void SetButtonsInteractable(bool isInteractable) => Buttons.ForEach(b => b.SetInteractable(isInteractable));

        public void Close() => SetActive(false);

        protected abstract string GetItemIdentification(T item);

        protected virtual void OnItemClicked_internal(T item)
        {
            if (SetClickedItemSelected)
            {
                string identification = GetItemIdentification(item);
            
                foreach (TButton button in Buttons)
                {
                    if (GetItemIdentification(button.displayedItem) != identification)
                    {
                        button.SetSelected(false);
                    }
                }
            }
            
            OnItemClicked.Invoke(item);
        }

        protected void OnCancelClicked_internal()
        {
            SetActive(false);
        }
    }
}