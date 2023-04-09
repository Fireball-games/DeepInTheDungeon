using System.Collections.Generic;
using System.Linq;
using Scripts.InventoryManagement.Inventories;
using Scripts.System.Pooling;
using UnityEngine;

namespace Scripts.InventoryManagement.UI.Inventories
{
    /// <summary>
    /// To be placed on the root of the inventory UI. Handles spawning all the
    /// inventory slot prefabs.
    /// </summary>
    public class InventoryUI : InventoryUIBase
    {
        [SerializeField] private InventorySlotUI inventoryItemPrefab;

        private Inventory _playerInventory;
        private Transform _itemsParent;

        protected override void Awake() 
        {
            base.Awake();
            
            _playerInventory = Inventory.PlayerInventory;
            _itemsParent = transform.Find("Background/Frame/ScrollView/Viewport/Content");
        }

        protected override void SetTitle()
        {
        }

        private void Start()
        {
            Redraw();
        }

        public override void OnInitialize()
        {
            _playerInventory.OnInventoryUpdated.AddListener(Redraw);
        }

        private void Redraw()
        {
            IEnumerable<InventorySlotUI> slots = _itemsParent.GetComponentsInChildren<InventorySlotUI>();
            
            if (slots.Count() > _playerInventory.InventorySize)
            {
                for (int i = _playerInventory.InventorySize; i < slots.Count(); i++)
                {
                    slots.ElementAt(i).gameObject.DismissToPool();
                }
            }
            
            for (int i = 0; i < _playerInventory.InventorySize; i++)
            {
                if (i >= slots.Count())
                {
                    slots = slots.Append(inventoryItemPrefab.GetFromPool(_itemsParent));
                }
                
                slots.ElementAt(i).Setup(_playerInventory, i);
            }
        }
    }
}