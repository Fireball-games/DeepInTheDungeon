using NaughtyAttributes;
using Scripts.Building.ItemSpawning;
using Scripts.InventoryManagement.Inventories.Items;
using Scripts.Player;
using Scripts.System;

namespace Scripts.InventoryManagement.Inventories
{
    /// <summary>
    /// To be placed at the root of a Pickup prefab. Contains the data about the
    /// pickup such as the type of item and the number.
    /// </summary>
    public class Pickup : MapObjectInstance
    {
        [ShowNativeProperty]
        public int StackSize { get; private set; } = 1;

        private Inventory _inventory;

        private void OnEnable()
        {
            if (GameManager.IsInPlayMode && !_inventory)
            {
                _inventory = PlayerController.Instance.InventoryManager.Inventory;
            }
        }

        /// <summary>
        /// Set the vital data after creating the prefab.
        /// </summary>
        /// <param name="itemData">The type of item this prefab represents.</param>
        /// <param name="number">The number of items represented.</param>
        public void Setup(InventoryItem itemData, int number)
        {
            base.Setup(itemData);
            
            if (!itemData.IsStackable())
            {
                number = 1;
            }
            
            StackSize = number;
        }

        public int GetNumber()
        {
            return StackSize;
        }

        public void PickupItem()
        {
            if (!_inventory) return;
            
            bool foundSlot = _inventory.AddToFirstEmptySlot(Item as InventoryItem, StackSize);
            
            if (foundSlot)
            {
                Destroy(gameObject);
            }
        }

        public bool CanBePickedUp()
        {
            return _inventory.HasSpaceFor(Item as InventoryItem);
        }
    }
}