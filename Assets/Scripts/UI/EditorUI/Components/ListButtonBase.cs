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
         [SerializeField] private Image iconPrefab;
        protected T displayedPrefab;

        protected TMP_Text Text;
        
        private Button _button;
        private Color SelectedColor => Colors.Selected;
        
        private readonly Color _normalColor = Color.white;

        protected UnityEvent<T> OnClick { get; } = new();

        private void Awake()
        {
            Initialize();
        }

        private void OnEnable()
        {
            Text.gameObject.DismissAllChildrenToPool(true);
        }

        public virtual void Set(T prefab, UnityAction<T> onClick)
        {
            if(!Text) Initialize();
            
            displayedPrefab = prefab;
            OnClick.RemoveAllListeners();
            OnClick.AddListener(onClick);

            
            Text.color = _normalColor;
            SetItemName();
        }

        public void SetSelected(bool isSelected) => Text.color = isSelected ? SelectedColor : _normalColor;

        protected virtual void SetItemName()
        {
            if(!Text) Initialize();
        }

        private void AddIcon(EIcon icon)
        {
            Image newIcon = ObjectPool.Instance
                .GetFromPool(iconPrefab.gameObject, Text.gameObject, true)
                .GetComponent<Image>();

            newIcon.sprite = Get(icon);
        }

        protected virtual void OnClick_internal()
        {
            Text.color = SelectedColor;
            OnClick.Invoke(displayedPrefab);
        }

        protected void Initialize()
        {
            _button = transform.Find("Button").GetComponent<Button>();
            _button.onClick.AddListener(OnClick_internal);
            
            Text = transform.Find("Button/Text").GetComponent<TMP_Text>();
        }
    }
}