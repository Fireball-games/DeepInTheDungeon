using Scripts.InventoryManagement.UI.Inventories;
using UnityEngine;
using UnityEngine.Events;

namespace Scripts.InventoryManagement.Inventories
{
    public abstract class InventoryBase<TInventoryUI> : MonoBehaviour where TInventoryUI : InventoryUIBase
    {
        private InventoryUIBase _inventoryUi;
        
        /// <summary>
        /// Broadcasts when the items in the slots are added/removed.
        /// </summary>
        public UnityEvent OnInventoryUpdated = new();
        
        protected virtual void Awake()
        {
            AssignComponents();
        }

        public virtual void Initialize()
        {
            Clear();
            OnInventoryUpdated.RemoveAllListeners();
            _inventoryUi = FindObjectOfType<TInventoryUI>();
            
            if (_inventoryUi)
            {
                _inventoryUi.OnInitialize();
            }
        }

        public abstract void Clear();

        public void ToggleInventory()
        {
            if (!_inventoryUi) AssignComponents();
            _inventoryUi.ToggleOpen();
        }
        
        public void Close()
        {
            // Can happen f.ex. in main screen, it's valid state
            if (!_inventoryUi) AssignComponents();
            if (!_inventoryUi) return;
            
            _inventoryUi.Close();
        }

        protected virtual void AssignComponents()
        {
        }
    }
}