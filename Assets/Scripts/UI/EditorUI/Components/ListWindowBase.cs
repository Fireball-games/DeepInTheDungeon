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

        protected HashSet<TButton> _buttons;

        protected UnityEvent<T> OnItemClicked { get; } = new();
        protected UnityEvent OnCancelClicked { get; } = new();

        protected virtual void Awake()
        {
            _buttons = new HashSet<TButton>();
        }

        public void Open(string listTitle, IEnumerable<T> prefabs, UnityAction<T> onItemClicked, UnityAction onClose = null)
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

            _buttons ??= new HashSet<TButton>();
            _buttons.Clear();

            if (prefabs == null) return;

            foreach (T prefab in prefabs)  
            {
                TButton newButton = ObjectPool.Instance
                    .GetFromPool(itemPrefab, listContent)
                    .GetComponent<TButton>();
                
                newButton.Set(prefab, OnItemClicked_internal);

                _buttons.Add(newButton);
            }
        }

        public void DeselectButtons() => _buttons.ForEach(b => b.SetSelected(false));

        public void Close() => SetActive(false);

        protected abstract void OnItemClicked_internal(T item);

        protected virtual void OnCancelClicked_internal()
        {
            OnCancelClicked.Invoke();
        }
    }
}