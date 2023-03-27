using NaughtyAttributes;
using Scripts.Building.ItemSpawning;
using Scripts.EventsManagement;
using Scripts.InventoryManagement.Inventories.Items;
using Scripts.Player;
using Scripts.System;
using UnityEngine;

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

        [ReadOnly, SerializeField] private bool IsPickedUp;
        private Inventory _inventory;
        private PlayerController _player => PlayerController.Instance;

        private void OnEnable()
        {
            IsPickedUp = false;
            
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
            
            IsPickedUp = true;
            
            bool foundSlot = _inventory.AddToFirstEmptySlot(Item as InventoryItem, StackSize);
            
            if (foundSlot)
            {
                EventsManager.TriggerOnMapObjectRemovedFromMap(this);
                Destroy(gameObject);
            }
        }

        public bool CanBePickedUp()
        {
            return !IsPickedUp && IsPlayerInRange() && _inventory.HasSpaceFor(Item as InventoryItem);
        }
        
        private bool IsPlayerInRange()
        {
            return Vector3.SqrMagnitude(_player.transform.position - transform.position) < PlayerInventoryManager.MaxClickPickupDistance;
        }
    }
}