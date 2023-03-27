﻿using System;
using Scripts.InventoryManagement.Inventories.Items;
using Scripts.InventoryManagement.UI.Inventories;
using Scripts.Player;
using Scripts.System.Saving;
using UnityEngine;
using UnityEngine.Events;

namespace Scripts.InventoryManagement.Inventories
{
    /// <summary>
    /// Provides storage for the player inventory. A configurable number of
    /// slots are available.
    ///
    /// This component should be placed on the GameObject tagged "Player".
    /// </summary>
    public class Inventory : MonoBehaviour, ISavable
    {
        [Tooltip("Allowed size")]
        [SerializeField]
        private int inventorySize = 16;

        private InventoryUI _inventoryUI;

        private InventorySlot[] _slots;

        private struct InventorySlot
        {
            public InventoryItem Item;
            public int Number;
        }

        /// <summary>
        /// Broadcasts when the items in the slots are added/removed.
        /// </summary>
        public UnityEvent OnInventoryUpdated = new();
        
        private void Awake()
        {
            AssignComponents();
        }

        /// <summary>
        /// Convenience for getting the player's inventory.
        /// </summary>
        public static Inventory GetPlayerInventory() => PlayerController.Instance.InventoryManager.Inventory;

        /// <summary>
        /// Could this item fit anywhere in the inventory?
        /// </summary>
        public bool HasSpaceFor(InventoryItem item)
        {
            return FindSlot(item) >= 0;
        }

        /// <summary>
        /// How many slots are in the inventory?
        /// </summary>
        public int GetSize()
        {
            return _slots.Length;
        }

        /// <summary>
        /// Attempt to add the items to the first available slot.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <param name="number">The number to add.</param>
        /// <returns>Whether or not the item could be added.</returns>
        public bool AddToFirstEmptySlot(InventoryItem item, int number)
        {
            int i = FindSlot(item);

            if (i < 0)
            {
                return false;
            }

            _slots[i].Item = item;
            _slots[i].Number += number;
            
            OnInventoryUpdated.Invoke();
            
            return true;
        }

        /// <summary>
        /// Is there an instance of the item in the inventory?
        /// </summary>
        public bool HasItem(InventoryItem item)
        {
            for (int i = 0; i < _slots.Length; i++)
            {
                if (ReferenceEquals(_slots[i].Item, item))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Return the item type in the given slot.
        /// </summary>
        public InventoryItem GetItemInSlot(int slot)
        {
            return _slots[slot].Item;
        }

        /// <summary>
        /// Get the number of items in the given slot.
        /// </summary>
        public int GetNumberInSlot(int slot)
        {
            return _slots[slot].Number;
        }

        /// <summary>
        /// Remove a number of items from the given slot. Will never remove more
        /// that there are.
        /// </summary>
        public void RemoveFromSlot(int slot, int number)
        {
            _slots[slot].Number -= number;
            if (_slots[slot].Number <= 0)
            {
                _slots[slot].Number = 0;
                _slots[slot].Item = null;
            }
            
            OnInventoryUpdated?.Invoke();
        }

        /// <summary>
        /// Will add an item to the given slot if possible. If there is already
        /// a stack of this type, it will add to the existing stack. Otherwise,
        /// it will be added to the first empty slot.
        /// </summary>
        /// <param name="slot">The slot to attempt to add to.</param>
        /// <param name="item">The item type to add.</param>
        /// <param name="number">The number of items to add.</param>
        /// <returns>True if the item was added anywhere in the inventory.</returns>
        public bool AddItemToSlot(int slot, InventoryItem item, int number)
        {
            if (_slots[slot].Item != null)
            {
                return AddToFirstEmptySlot(item, number);
            }

            int i = FindStack(item);
            if (i >= 0)
            {
                slot = i;
            }

            _slots[slot].Item = item;
            _slots[slot].Number += number;
            
            OnInventoryUpdated.Invoke();
            
            return true;
        }
        
        public void ToggleInventory()
        {
            if (!_inventoryUI) AssignComponents();
            _inventoryUI.ToggleOpen();
        }

        /// <summary>
        /// Find a slot that can accomodate the given item.
        /// </summary>
        /// <returns>-1 if no slot is found.</returns>
        private int FindSlot(InventoryItem item)
        {
            int i = FindStack(item);
            if (i < 0)
            {
                i = FindEmptySlot();
            }
            return i;
        }

        /// <summary>
        /// Find an empty slot.
        /// </summary>
        /// <returns>-1 if all slots are full.</returns>
        private int FindEmptySlot()
        {
            for (int i = 0; i < _slots.Length; i++)
            {
                if (_slots[i].Item == null)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Find an existing stack of this item type.
        /// </summary>
        /// <returns>-1 if no stack exists or if the item is not stackable.</returns>
        private int FindStack(InventoryItem item)
        {
            if (!item.IsStackable())
            {
                return -1;
            }

            for (int i = 0; i < _slots.Length; i++)
            {
                if (ReferenceEquals(_slots[i].Item, item))
                {
                    return i;
                }
            }
            return -1;
        }

        [Serializable]
        private struct InventorySlotRecord
        {
            public string itemID;
            public int number;
        }

        public string Guid { get; set; }

        object ISavable.CaptureState()
        {
            InventorySlotRecord[] slotStrings = new InventorySlotRecord[inventorySize];
            for (int i = 0; i < inventorySize; i++)
            {
                if (_slots[i].Item != null)
                {
                    slotStrings[i].itemID = _slots[i].Item.GetItemID();
                    slotStrings[i].number = _slots[i].Number;
                }
            }
            return slotStrings;
        }

        void ISavable.RestoreState(object state)
        {
            InventorySlotRecord[] slotStrings = (InventorySlotRecord[])state;
            for (int i = 0; i < inventorySize; i++)
            {
                _slots[i].Item = InventoryItem.GetFromID<InventoryItem>(slotStrings[i].itemID);
                _slots[i].Number = slotStrings[i].number;
            }

            OnInventoryUpdated.Invoke();
        }

        private void AssignComponents()
        {
            _slots = new InventorySlot[inventorySize];
            _inventoryUI = FindObjectOfType<InventoryUI>();
        }
    }
}