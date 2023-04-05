using System;
using Scripts.InventoryManagement.UI.Inventories;
using UnityEngine;
using UnityEngine.Events;

namespace Scripts.InventoryManagement.Inventories
{
    public abstract class InventoryBase<TInventory> : MonoBehaviour where TInventory : InventoryUIBase
    {
        private InventoryUIBase _inventoryUi;
        
        /// <summary>
        /// Broadcasts when the items in the slots are added/removed.
        /// </summary>
        public UnityEvent OnInventoryUpdated = new();
        
        private void Awake()
        {
            AssignComponents();
        }

        private void OnEnable()
        {
            OnInventoryUpdated.RemoveAllListeners();
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