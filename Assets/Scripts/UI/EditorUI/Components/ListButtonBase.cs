﻿using Scripts.Helpers;
using Scripts.Helpers.Extensions;
using Scripts.System.Pooling;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static IconStore;

namespace Scripts.UI.EditorUI.Components
{
    public abstract class ListButtonBase<T> : MonoBehaviour
    {
        [SerializeField] private Image iconPrefab;
        public T displayedItem;

        protected TMP_Text Text;
        
        private Button _button;
        private Color SelectedColor => Colors.Selected;
        
        private readonly Color _normalColor = Color.white;

        protected UnityEvent<T> OnClick { get; } = new();

        protected virtual void Awake()
        {
            Initialize();
        }

        public virtual void Set(T item, UnityAction<T> onClick)
        {
            if(!Text) Initialize();
            
            Text.gameObject.DismissAllChildrenToPool(true);
            
            displayedItem = item;
            OnClick.RemoveAllListeners();
            OnClick.AddListener(onClick);
            
            Text.color = _normalColor;
        }

        public void SetSelected(bool isSelected) => Text.color = isSelected ? SelectedColor : _normalColor;

        protected void AddIcon(EIcon icon)
        {
            Image newIcon = ObjectPool.Instance
                .GetFromPool(iconPrefab.gameObject, Text.gameObject)
                .GetComponent<Image>();

            newIcon.sprite = Get(icon);
        }

        protected virtual void OnClick_internal()
        {
            Text.color = SelectedColor;
            OnClick.Invoke(displayedItem);
        }

        private void Initialize()
        {
            _button = transform.Find("Button").GetComponent<Button>();
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(OnClick_internal);
            
            Text = transform.Find("Button/Text").GetComponent<TMP_Text>();
        }
    }
}