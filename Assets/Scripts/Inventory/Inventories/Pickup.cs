using Scripts.Building.ItemSpawning;
using Scripts.Inventory.Inventories.Items;
using Scripts.System;
using Scripts.System.MonoBases;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Android;

namespace Scripts.Inventory.Inventories
{
    /// <summary>
    /// To be placed at the root of a Pickup prefab. Contains the data about the
    /// pickup such as the type of item and the number.
    /// </summary>
    public class Pickup : MapObjectInstance
    {
        public int StackSize => _number;
        
        private InventoryItem _item;
        private int _number = 1;

        private Inventory _inventory;

        private void Awake()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            
            if (GameManager.IsInPlayMode)
            {
                _inventory = player.GetComponent<Inventory>();
            }
        }

        /// <summary>
        /// Set the vital data after creating the prefab.
        /// </summary>
        /// <param name="item">The type of item this prefab represents.</param>
        /// <param name="number">The number of items represented.</param>
        public void Setup(InventoryItem item, int number)
        {
            base.Setup(item);
            
            if (!item.IsStackable())
            {
                number = 1;
            }
            
            _number = number;
        }

        public int GetNumber()
        {
            return _number;
        }

        public void PickupItem()
        {
            bool foundSlot = _inventory.AddToFirstEmptySlot(_item, _number);
            if (foundSlot)
            {
                Destroy(gameObject);
            }
        }

        public bool CanBePickedUp()
        {
            return _inventory.HasSpaceFor(_item);
        }
    }
}