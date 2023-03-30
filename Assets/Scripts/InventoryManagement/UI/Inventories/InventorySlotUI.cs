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
        [ReadOnly] public int index;
        private InventoryItem _item;
        private Inventory _inventory;

        // PUBLIC

        public void Setup(Inventory inventory, int slotIndex)
        {
            _inventory = inventory;
            index = slotIndex;
            icon.SetItem(inventory.GetItemInSlot(slotIndex), inventory.GetNumberInSlot(slotIndex));
        }

        public int MaxAcceptable(InventoryItem item)
        {
            return _inventory.HasSpaceFor(item) 
                ? int.MaxValue 
                : 0;
        }

        public void AddItem(InventoryItem item, int number)
        {
            _inventory.AddItemToSlot(index, item, number);
        }

        public InventoryItem GetItem()
        {
            return _inventory.GetItemInSlot(index);
        }

        public int GetNumber()
        {
            return _inventory.GetNumberInSlot(index);
        }

        public void RemoveItems(int number)
        {
            _inventory.RemoveFromSlot(index, number);
        }
    }
}