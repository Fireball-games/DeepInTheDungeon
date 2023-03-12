using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Inventory.Inventories.Items
{
    public class MapObject : ScriptableObject, ISerializationCallbackReceiver
    {
        // CONFIG DATA
        
        /// Auto-generated UUID for saving/loading. Clear this field if you want to generate a new one.
        protected string ItemID;
        /// Item name to be displayed in UI.
        [SerializeField] private  string displayName;
        public string DisplayName => displayName;

        // STATE
        private static Dictionary<string, MapObject> _itemLookupCache;

        // PUBLIC

        /// <summary>
        /// Get the inventory item instance from its UUID.
        /// </summary>
        /// <param name="itemID">
        /// String UUID that persists between game instances.
        /// </param>
        /// <returns>
        /// Inventory item instance corresponding to the ID.
        /// </returns>
        public static T GetFromID<T>(string itemID) where T : MapObject
        {
            if (_itemLookupCache == null)
            {
                _itemLookupCache = new Dictionary<string, MapObject>();
                MapObject[] itemList = Resources.LoadAll<MapObject>("");
                foreach (MapObject item in itemList)
                {
                    if (_itemLookupCache.ContainsKey(item.ItemID))
                    {
                        Debug.LogError(
                            $"Looks like there's a duplicate GameDevTV.UI.InventorySystem ID for objects: {_itemLookupCache[item.ItemID]} and {item}");
                        continue;
                    }

                    _itemLookupCache[item.ItemID] = item;
                }
            }

            if (itemID == null || !_itemLookupCache.ContainsKey(itemID)) return null;
            return _itemLookupCache[itemID] as T;
        }

        public string GetItemID()
        {
            return ItemID;
        }

        
        public string GetDisplayName()
        {
            return displayName;
        }

        // PRIVATE
        
        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            // Generate and save a new UUID if this is blank.
            if (string.IsNullOrWhiteSpace(ItemID))
            {
                ItemID = Guid.NewGuid().ToString();
            }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            // Require by the ISerializationCallbackReceiver but we don't need
            // to do anything with it.
        }
    }
}