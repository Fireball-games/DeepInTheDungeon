using Scripts.InventoryManagement.UI.Inventories;
using UnityEngine;

namespace Scripts.InventoryManagement.Inventories
{
    public abstract class InventoryBase<TInventory> : MonoBehaviour where TInventory : InventoryUIBase
    {
        private InventoryUIBase _inventoryUi;
        
        private void Awake()
        {
            AssignComponents();
        }
        
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
            _inventoryUi = FindObjectOfType<TInventory>();
        }
    }
}