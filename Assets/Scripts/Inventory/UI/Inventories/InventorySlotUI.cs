using Scripts.Inventory.Inventories.Items;
using Scripts.Inventory.Utils.UI.Dragging;
using UnityEngine;

namespace Scripts.Inventory.UI.Inventories
{
    public class InventorySlotUI : MonoBehaviour, IItemHolder, IDragContainer<InventoryItem>
    {
        // CONFIG DATA
        [SerializeField] private InventoryItemIcon icon;

        // STATE
        private int _index;
        private InventoryItem _item;
        private Inventory.Inventories.Inventory _inventory;

        // PUBLIC

        public void Setup(Inventory.Inventories.Inventory inventory, int index)
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

        public void AddItems(InventoryItem item, int number)
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