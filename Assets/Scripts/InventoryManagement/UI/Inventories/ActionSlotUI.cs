using Scripts.InventoryManagement.Inventories;
using Scripts.InventoryManagement.Inventories.Items;
using Scripts.InventoryManagement.Utils.UI.Dragging;
using Scripts.Player;
using UnityEngine;
using UnityEngine.Events;

namespace Scripts.InventoryManagement.UI.Inventories
{
    /// <summary>
    /// The UI slot for the player action bar.
    /// </summary>
    public class ActionSlotUI : MonoBehaviour, IItemHolder, IDragContainer<InventoryItem>
    {
        [SerializeField] private int index;
        private InventoryItemIcon _icon;

        private PlayerController Player => PlayerController.Instance;
        
        private static ActionStore _store;

        private static readonly UnityEvent Initialize = new();
        public static void TriggerInitialization() => Initialize.Invoke();
        
        private void Awake()
        {
            Initialize.AddListener(OnInitialize);
        }
        
        private void OnEnable()
        {
            _store ??= Player.InventoryManager.ActionStore;
        }

        public void AddItem(InventoryItem item, int number)
        {
            _store.AddAction(item, index, number);
        }

        public InventoryItem GetItem()
        {
            return _store.GetAction(index);
        }

        public int GetNumber()
        {
            return _store.GetNumber(index);
        }

        public int MaxAcceptable(InventoryItem item)
        {
            return _store.MaxAcceptable(item, index);
        }

        public void RemoveItems(int number)
        {
            _store.RemoveItems(index, number);
        }
        
        private void OnInitialize()
        {
            _store.OnStoreUpdated.AddListener(UpdateIcon);
            _icon ??= GetComponentInChildren<InventoryItemIcon>();
        }

        private void UpdateIcon()
        {
            _icon.SetItem(GetItem(), GetNumber());
        }
    }
}
