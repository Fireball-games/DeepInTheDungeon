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

        private void Awake() 
        {
            _playerInventory = Inventory.GetPlayerInventory();
            _itemsParent = transform.Find("Background/Frame/ScrollView/Viewport/Content");
        }

        private void OnEnable()
        {
            _playerInventory.OnInventoryUpdated.AddListener(Redraw);
        }

        private void Start()
        {
            Redraw();
        }
        
        private void OnDisable()
        {
            _playerInventory.OnInventoryUpdated.RemoveListener(Redraw);
        }

        private void Redraw()
        {
            // _itemsParent.gameObject.DismissAllChildrenToPool();
            
            IEnumerable<InventorySlotUI> slots = _itemsParent.GetComponentsInChildren<InventorySlotUI>();
            
            if (slots.Count() > _playerInventory.GetSize())
            {
                for (int i = _playerInventory.GetSize(); i < slots.Count(); i++)
                {
                    slots.ElementAt(i).gameObject.DismissToPool();
                }
            }
            
            for (int i = 0; i < _playerInventory.GetSize(); i++)
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