using System;
using System.Collections.Generic;
using Scripts.InventoryManagement.Inventories.Items;
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
        public UnityEvent storeUpdated = new();

        private void Awake()
        {
            Guid = "ActionStore";
            storeUpdated.RemoveAllListeners();
        }

        /// <summary>
        /// Get the action at the given index.
        /// </summary>
        public InventoryItem GetAction(int index)
        {
            if (_dockedItems.ContainsKey(index))
            {
                return _dockedItems[index].Item;
            }
            return null;
        }

        /// <summary>
        /// Get the number of items left at the given index.
        /// </summary>
        /// <returns>
        /// Will return 0 if no item is in the index or the item has
        /// been fully consumed.
        /// </returns>
        public int GetNumber(int index)
        {
            if (_dockedItems.ContainsKey(index))
            {
                return _dockedItems[index].Number;
            }
            return 0;
        }

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

            storeUpdated?.Invoke();
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
                    storeUpdated?.Invoke();
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
            for (int i = 0; i < _dockedItems.Count; i++)
            {
                _dockedItems.Remove(i);
            }
        }

        object ISavable.CaptureState()
        {
            Dictionary<int, DockedItemRecord> state = new();
            foreach (KeyValuePair<int, DockedItemSlot> pair in _dockedItems)
            {
                DockedItemRecord record = new()
                {
                    itemID = pair.Value.Item.GetItemID(),
                    number = pair.Value.Number
                };
                state[pair.Key] = record;
            }
            return state;
        }

        void ISavable.RestoreState(object state)
        {
            Dictionary<int, DockedItemRecord> stateDict = (Dictionary<int, DockedItemRecord>)state;
            foreach (KeyValuePair<int, DockedItemRecord> pair in stateDict)
            {
                AddAction(MapObject.GetFromID<InventoryItem>(pair.Value.itemID), pair.Key, pair.Value.number);
            }
        }
        
        [Serializable]
        private struct DockedItemRecord
        {
            public string itemID;
            public int number;
        }
    }
}