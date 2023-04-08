using System;
using System.Collections.Generic;
using System.Linq;
using Scripts.Helpers.Extensions;
using Scripts.InventoryManagement.Inventories.Items;
using Scripts.InventoryManagement.UI.Inventories;
using Scripts.System.Saving;
using UnityEngine;
using UnityEngine.Serialization;

namespace Scripts.InventoryManagement.Inventories
{
    /// <summary>
    /// Provides a store for the items equipped to a player. Items are stored by
    /// their equip locations.
    /// 
    /// This component should be placed on the GameObject tagged "Player".
    /// </summary>
    public class Equipment : InventoryBase<EquipmentUI>, ISavable
    {
        private Dictionary<EquipLocation, EquipableItem> _equippedItems = new();
        private EquipmentUI _equipmentUI;
        
        public string Guid { get; set; }

        protected override void Awake()
        {
            base.Awake();
            
            Guid = "Equipment";
        }
        
        public override void Initialize()
        {
            base.Initialize();
            
            FindObjectsOfType<EquipmentSlotUI>(true).ForEach(slot => slot.OnInitialize());
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
            Debug.Assert(item.GetAllowedEquipLocation().HasFlag(slot));

            _equippedItems[slot] = item;

            OnInventoryUpdated.Invoke();
        }

        /// <summary>
        /// Remove the item for the given slot.
        /// </summary>
        public void RemoveItem(EquipLocation slot, bool fireUpdateEvent = true)
        {
            _equippedItems.Remove(slot);
            
            if (fireUpdateEvent)
            {
                OnInventoryUpdated?.Invoke();
            }
        }
        
        public override void Clear()
        {
            if (_equippedItems == null || _equippedItems.Count == 0) return;
            
            new List<EquipLocation>(GetAllPopulatedSlots()).ForEach(location => RemoveItem(location, false));
        }

        /// <summary>
        /// Enumerate through all the slots that currently contain items.
        /// </summary>
        private IEnumerable<EquipLocation> GetAllPopulatedSlots()
        {
            return _equippedItems.Keys;
        }

        object ISavable.CaptureState()
        {
            return new EquipmentRecord(_equippedItems.Select(item => new EquipableSlotRecord(item.Key, item.Value)).ToList());
        }

        void ISavable.RestoreState(object state)
        {
            _equippedItems = new Dictionary<EquipLocation, EquipableItem>();

            EquipmentRecord equipmentRecord = (EquipmentRecord)state;

            foreach (EquipableSlotRecord slotRecord in equipmentRecord.equippedItems)
            {
                EquipableItem item = MapObject.GetFromID<EquipableItem>(slotRecord.itemID);
                if (item != null)
                {
                    AddItem(slotRecord.equipLocation, item);
                }
            }
        }
        
        [Serializable]
        private struct EquipmentRecord
        {
            public List<EquipableSlotRecord> equippedItems;

            public EquipmentRecord(List<EquipableSlotRecord> equippedItems)
            {
                this.equippedItems = equippedItems;
            }
        }
        
        [Serializable]
        private struct EquipableSlotRecord
        {
            [FormerlySerializedAs("slot")] public EquipLocation equipLocation;
            public string itemID;

            public EquipableSlotRecord(EquipLocation equipLocation, EquipableItem item)
            {
                this.equipLocation = equipLocation;
                itemID = item.GetItemID();
            }
        }
    }
}