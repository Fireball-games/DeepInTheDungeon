using NaughtyAttributes;
using Scripts.InventoryManagement.Inventories;
using Scripts.InventoryManagement.Inventories.Items;
using Scripts.InventoryManagement.Utils.UI.Dragging;
using UnityEngine;

namespace Scripts.InventoryManagement.UI.Inventories
{
    public class InventorySlotUI : MonoBehaviour, IItemHolder, IDragContainer<InventoryItem>
    {
        // CONFIG DATA
        [SerializeField] private InventoryItemIcon icon;

        // STATE
        [ReadOnly] public int _index;
        private InventoryItem _item;
        private Inventory _inventory;

        // PUBLIC

        public void Setup(Inventory inventory, int index)
        {
            _inventory = inventory;
            _index = index;
            icon.SetItem(inventory.GetItemInSlot(index), inventory.GetNumberInSlot(index));
        }

        public int MaxAcceptable(InventoryItem item)
        {
            if (_inventory.HasSpaceFor(item))
            {
                return int.MaxValue;
            }
            return 0;
        }

        public void AddItem(InventoryItem item, int number)
        {
            _inventory.AddItemToSlot(_index, item, number);
        }

        public InventoryItem GetItem()
        {
            return _inventory.GetItemInSlot(_index);
        }

        public int GetNumber()
        {
            return _inventory.GetNumberInSlot(_index);
        }

        public void RemoveItems(int number)
        {
            _inventory.RemoveFromSlot(_index, number);
        }
    }
}