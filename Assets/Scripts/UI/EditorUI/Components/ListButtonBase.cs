using System;
using NaughtyAttributes;
using Scripts.Helpers;
using Scripts.Helpers.Extensions;
using Scripts.System.Pooling;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static Scripts.UI.IconStore;

namespace Scripts.UI.EditorUI.Components
{
    public abstract class ListButtonBase<T> : MonoBehaviour
    {
        public bool MarkSelectedOnClick = true;
        [NonSerialized] public IListWindow ParentList;
        [SerializeField] private Image iconPrefab;
        [ReadOnly] public T displayedItem;

        protected bool IsSelected;

        protected TMP_Text Text;
        protected Button Button;
        private Color SelectedColor => Colors.Selected;
        private readonly Color _normalColor = Color.white;

        private UnityEvent<T> OnClick { get; } = new();

        protected virtual void Awake()
        {
            Initialize();
        }

        public virtual void Set(T item, UnityAction<T> onClick, bool setSelectedOnClick = true)
        {
            if(!Text) Initialize();

            if (Text)
            {
                Text.gameObject.DismissAllChildrenToPool();
            }
            
            SetInteractable(true);
            
            displayedItem = item;
            OnClick.RemoveAllListeners();

            MarkSelectedOnClick = setSelectedOnClick;
            
            if (onClick != null)
            {
                OnClick.AddListener(onClick);
            }

            if (Text)
            {
                Text.color = _normalColor;
            }
        }

        public virtual void SetSelected(bool isSelected)
        {
            IsSelected = isSelected;
            if (!Text) return;
            Text.color = isSelected ? SelectedColor : _normalColor;
        }

        public void SetInteractable(bool isInteractable) => Button.interactable = isInteractable;

        protected void AddIcon(EIcon icon)
        {
            Image newIcon = ObjectPool.Instance
                .Get(iconPrefab.gameObject, Text.gameObject)
                .GetComponent<Image>();

            newIcon.sprite = Get(icon);
        }

        protected virtual void OnClick_internal()
        {
            if (MarkSelectedOnClick && Text)
            {
                Text.color = SelectedColor;
            }
            
            OnClick.Invoke(displayedItem);
        }

        protected virtual void AssignComponents()
        {
            Button = transform.Find("Button").GetComponent<Button>();
            Text = transform.Find("Button/Text")?.GetComponent<TMP_Text>();
        }

        private void Initialize()
        {
            AssignComponents();
            Button.onClick.RemoveAllListeners();
            Button.onClick.AddListener(OnClick_internal);
        }
    }
}