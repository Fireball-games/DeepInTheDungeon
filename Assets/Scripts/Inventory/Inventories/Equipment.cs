using System;
using System.Collections.Generic;
using Scripts.Inventory.Inventories.Items;
using Scripts.System.Saving;
using UnityEngine;

namespace Scripts.Inventory.Inventories
{
    /// <summary>
    /// Provides a store for the items equipped to a player. Items are stored by
    /// their equip locations.
    /// 
    /// This component should be placed on the GameObject tagged "Player".
    /// </summary>
    public class Equipment : MonoBehaviour, ISavable
    {
        // STATE
        private Dictionary<EquipLocation, EquipableItem> _equippedItems = new();

        // PUBLIC

        /// <summary>
        /// Broadcasts when the items in the slots are added/removed.
        /// </summary>
        public event Action equipmentUpdated;

        private void Awake()
        {
            Guid = global::System.Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Return the item in the given equip location.
        /// </summary>
        public EquipableItem GetItemInSlot(EquipLocation equipLocation)
        {
            if (!_equippedItems.ContainsKey(equipLocation))
            {
                return null;
            }

            return _equippedItems[equipLocation];
        }

        /// <summary>
        /// Add an item to the given equip location. Do not attempt to equip to
        /// an incompatible slot.
        /// </summary>
        public void AddItem(EquipLocation slot, EquipableItem item)
        {
            Debug.Assert(item.GetAllowedEquipLocation() == slot);

            _equippedItems[slot] = item;

            if (equipmentUpdated != null)
            {
                equipmentUpdated();
            }
        }

        /// <summary>
        /// Remove the item for the given slot.
        /// </summary>
        public void RemoveItem(EquipLocation slot)
        {
            _equippedItems.Remove(slot);
            if (equipmentUpdated != null)
            {
                equipmentUpdated();
            }
        }

        /// <summary>
        /// Enumerate through all the slots that currently contain items.
        /// </summary>
        public IEnumerable<EquipLocation> GetAllPopulatedSlots()
        {
            return _equippedItems.Keys;
        }

        // PRIVATE

        public string Guid { get; set; }

        object ISavable.CaptureState()
        {
            Dictionary<EquipLocation, string> equippedItemsForSerialization = new();
            foreach (KeyValuePair<EquipLocation, EquipableItem> pair in _equippedItems)
            {
                equippedItemsForSerialization[pair.Key] = pair.Value.GetItemID();
            }
            return equippedItemsForSerialization;
        }

        void ISavable.RestoreState(object state)
        {
            _equippedItems = new Dictionary<EquipLocation, EquipableItem>();

            Dictionary<EquipLocation, string> equippedItemsForSerialization = (Dictionary<EquipLocation, string>)state;

            foreach (KeyValuePair<EquipLocation, string> pair in equippedItemsForSerialization)
            {
                EquipableItem item = MapObject.GetFromID<EquipableItem>(pair.Value);
                if (item != null)
                {
                    _equippedItems[pair.Key] = item;
                }
            }
        }
    }
}