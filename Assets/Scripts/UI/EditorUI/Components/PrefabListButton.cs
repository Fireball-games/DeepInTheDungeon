using Scripts.Building.PrefabsSpawning.Walls.Identifications;
using Scripts.Building.Walls;
using Scripts.Helpers.Extensions;
using Scripts.System.Pooling;
using Scripts.Triggers;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static IconStore;

namespace Scripts.UI.EditorUI.Components
{
    public class PrefabListButton : MonoBehaviour
    {
        [SerializeField] private Color selectedColor = Color.green;
        [SerializeField] private Image iconPrefab;
        public PrefabBase displayedPrefab;

        private Button _button;
        private TMP_Text _text;
        
        private readonly Color _normalColor = Color.white;

        public UnityEvent<PrefabBase> OnClick { get; } = new();

        private void Awake()
        {
            _button = transform.Find("Button").GetComponent<Button>();
            _button.onClick.AddListener(OnClick_internal);
            
            _text = transform.Find("Button/Text").GetComponent<TMP_Text>();
        }

        private void OnEnable()
        {
            _text.gameObject.DismissAllChildrenToPool(true);
        }

        public void Set(PrefabBase prefab, UnityAction<PrefabBase> onClick)
        {
            displayedPrefab = prefab;
            OnClick.RemoveAllListeners();
            OnClick.AddListener(onClick);

            _text.color = _normalColor;
            _text.text = prefab.gameObject.name;

            switch (prefab)
            {
                case IMovementWall:
                {
                    AddIcon(EIcon.Move);
                    break;
                }
                case IWallWithTrigger:
                {
                    if (prefab.GetComponent<TriggerReceiver>()) AddIcon(EIcon.TriggerReceiver);
                    if (prefab.GetComponentInChildren<Trigger>()) AddIcon(EIcon.Trigger);
                    break;
                }
            }
        }

        public void SetSelected(bool isSelected) => _text.color = isSelected ? selectedColor : _normalColor;

        private void AddIcon(EIcon icon)
        {
            Image newIcon = ObjectPool.Instance
                .GetFromPool(iconPrefab.gameObject, _text.gameObject, true)
                .GetComponent<Image>();

            newIcon.sprite = Get(icon);
        }

        private void OnClick_internal()
        {
            _text.color = selectedColor;
            OnClick.Invoke(displayedPrefab);
        }
    }
}