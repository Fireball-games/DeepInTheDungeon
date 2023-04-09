using System;
using System.Collections.Generic;
using Scripts.InventoryManagement.Inventories.Items;
using Scripts.InventoryManagement.UI.Inventories;
using Scripts.System.Saving;
using UnityEngine;
using UnityEngine.Events;
using Logger = Scripts.Helpers.Logger;

namespace Scripts.InventoryManagement.Inventories
{
    /// <summary>
    /// Provides the storage for an action bar. The bar has a finite number of
    /// slots that can be filled and actions in the slots can be "used".
    /// 
    /// This component should be placed on the GameObject tagged "Player".
    /// </summary>
    public class ActionStore : MonoBehaviour, ISavable 
    {
        // STATE
        private readonly Dictionary<int, DockedItemSlot> _dockedItems = new();
        private class DockedItemSlot 
        {
            public InventoryItem Item;
            public int Number;
        }

        // PUBLIC

        /// <summary>
        /// Broadcasts when the items in the slots are added/removed.
        /// </summary>
        public UnityEvent OnStoreUpdated = new();

        private void Awake()
        {
            Guid = "ActionStore";
        }
        
        public void Initialize()
        {
            Clear();
            OnStoreUpdated.RemoveAllListeners();
            ActionSlotUI.TriggerInitialization();
        }

        /// <summary>
        /// Get the action at the given index.
        /// </summary>
        public InventoryItem GetAction(int index) =>
            _dockedItems.TryGetValue(index, out DockedItemSlot item) 
                ? item.Item 
                : null;

        /// <summary>
        /// Get the number of items left at the given index.
        /// </summary>
        /// <returns>
        /// Will return 0 if no item is in the index or the item has
        /// been fully consumed.
        /// </returns>
        public int GetNumber(int index) =>
            _dockedItems.TryGetValue(index, out DockedItemSlot item) 
                ? item.Number 
                : 0;

        /// <summary>
        /// Add an item to the given index.
        /// </summary>
        /// <param name="item">What item should be added.</param>
        /// <param name="index">Where should the item be added.</param>
        /// <param name="number">How many items to add.</param>
        public void AddAction(InventoryItem item, int index, int number)
        {
            if (_dockedItems.ContainsKey(index))
            {  
                if (ReferenceEquals(item, _dockedItems[index].Item))
                {
                    _dockedItems[index].Number += number;
                }
            }
            else
            {
                DockedItemSlot slot = new()
                {
                    Item = item,
                    Number = number
                };
                _dockedItems[index] = slot;
            }

            OnStoreUpdated.Invoke();
        }

        /// <summary>
        /// Use the item at the given slot. If the item is consumable one
        /// instance will be destroyed until the item is removed completely.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="user">The character that wants to use this action.</param>
        /// <returns>False if the action could not be executed.</returns>
        public bool Use(int index, GameObject user)
        {
            if (_dockedItems.ContainsKey(index))
            {
                InventoryItem item = _dockedItems[index].Item;
                if (item is ActionItem actionItem)
                {
                    actionItem.Use(user);
                    if (actionItem.IsConsumable())
                    {
                        RemoveItems(index, 1);
                    }
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Remove a given number of items from the given slot.
        /// </summary>
        public void RemoveItems(int index, int number, bool fireUpdateEvent = true)
        {
            if (_dockedItems.ContainsKey(index))
            {
                _dockedItems[index].Number -= number;
                if (_dockedItems[index].Number <= 0)
                {
                    _dockedItems.Remove(index);
                }

                if (fireUpdateEvent)
                {
                    OnStoreUpdated.Invoke();
                }
            }
            
        }

        /// <summary>
        /// What is the maximum number of items allowed in this slot.
        /// 
        /// This takes into account whether the slot already contains an item
        /// and whether it is the same type. Will only accept multiple if the
        /// item is consumable.
        /// </summary>
        /// <returns>Will return int.MaxValue when there is not effective bound.</returns>
        public int MaxAcceptable(InventoryItem item, int index)
        {
            if (!item.UseType.HasFlag(InventoryItem.EUseType.Use)) return 0;

            if (_dockedItems.ContainsKey(index) && !ReferenceEquals(item, _dockedItems[index].Item))
            {
                return 0;
            }
            
            if (item.IsStackable())
            {
                return item.MaxStackSize;
            }
            
            if (_dockedItems.ContainsKey(index))
            {
                return 0;
            }

            return 1;
        }

        public string Guid { get; set; }

        public void Close()
        {
            Logger.LogNotImplemented();
        }
        
        public void Clear()
        {
            if (_dockedItems == null || _dockedItems.Count == 0) return;
            
            _dockedItems.Clear();
        }

        object ISavable.CaptureState()
        {
            ActionStoreRecord record = new();
            foreach (KeyValuePair<int,DockedItemSlot> slot in _dockedItems)
            {
                record.slots.Add(new ActionSlotRecord(slot.Key, slot.Value));
            }
            return record;
        }

        void ISavable.RestoreState(object state)
        {
            ActionStoreRecord record = (ActionStoreRecord)state;
            
            if (record.slots == null) return;

            foreach (ActionSlotRecord slotRecord in record.slots)
            {
                AddAction(MapObject.GetFromID<InventoryItem>(slotRecord.slot.itemID), slotRecord.index, slotRecord.slot.number);
            }
        }
        
        [Serializable]
        private class ActionStoreRecord
        {
            public List<ActionSlotRecord> slots;
            
            public ActionStoreRecord()
            {
                slots = new List<ActionSlotRecord>();
            }
        }
        
        [Serializable]
        private struct ActionSlotRecord
        {
            public int index;
            public InventorySlotRecord slot;
            
            public ActionSlotRecord(int index, DockedItemSlot slot)
            {
                this.index = index;
                this.slot = new InventorySlotRecord(slot.Item.GetItemID(), slot.Number);
            }
        }
    }
}