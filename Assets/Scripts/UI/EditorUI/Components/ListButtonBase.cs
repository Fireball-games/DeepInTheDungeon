using Scripts.Helpers;
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
        public bool MarkSelectedOnClick = true;
        [SerializeField] private Image iconPrefab;
        public T displayedItem;

        protected TMP_Text Text;
        
        protected Button Button;
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
            
            if (onClick != null)
            {
                OnClick.AddListener(onClick);
            }
            
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
            if (MarkSelectedOnClick)
            {
                Text.color = SelectedColor;
            }
            
            OnClick.Invoke(displayedItem);
        }

        protected virtual void AssignComponents()
        {
            Button = transform.Find("Button").GetComponent<Button>();
            Text = transform.Find("Button/Text").GetComponent<TMP_Text>();
        }

        private void Initialize()
        {
            AssignComponents();
            Button.onClick.RemoveAllListeners();
            Button.onClick.AddListener(OnClick_internal);
            
        }
    }
}