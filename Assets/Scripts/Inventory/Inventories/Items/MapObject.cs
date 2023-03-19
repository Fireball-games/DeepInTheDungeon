using System;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using Logger = Scripts.Helpers.Logger;

namespace Scripts.Inventory.Inventories.Items
{
    public class MapObject : ScriptableObject, ISerializationCallbackReceiver
    {
        // CONFIG DATA
        
        /// Auto-generated UUID for saving/loading. Clear this field if you want to generate a new one.
        [ReadOnly, SerializeField] protected string itemID;
        /// Item name to be displayed in UI.
        [SerializeField] private  string displayName;
        [Tooltip("The UI icon to represent this item in the inventory or in the editor.")]
        [SerializeField] protected Sprite icon;
        [Tooltip("The prefab that should be spawned when this item appear in the map.")]
        [SerializeField] protected Pickup pickup;
        public string DisplayName => displayName;
        public Sprite Icon => icon;

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
            _itemLookupCache ??= BuildItemLookupCache();

            if (itemID == null || !_itemLookupCache.ContainsKey(itemID)) return null;
            return _itemLookupCache[itemID] as T;
        }

        public string GetItemID() => itemID;
        
        public static IEnumerable<MapObject> GetAllItems()
        {
            _itemLookupCache ??= BuildItemLookupCache();
            return _itemLookupCache.Values;
        }

        private static Dictionary<string, MapObject> BuildItemLookupCache()
        {
            if (_itemLookupCache == null)
            {
                _itemLookupCache = new Dictionary<string, MapObject>();
                MapObject[] itemList = Resources.LoadAll<MapObject>($"Items/MapObjects");
                
                if (!itemList.Any())
                {
                    Logger.LogError("No MapObjects found in Resources folder.");
                    return _itemLookupCache;
                }
                
                foreach (MapObject item in itemList)
                {
                    if (_itemLookupCache.ContainsKey(item.itemID))
                    {
                        Debug.LogError(
                            $"Looks like there's a duplicate ID for MapObjects: {_itemLookupCache[item.itemID]} and {item}");
                        continue;
                    }

                    _itemLookupCache[item.itemID] = item;
                }
            }

            return _itemLookupCache;
        }

        private void OnValidate() => CheckItemID();

        void ISerializationCallbackReceiver.OnBeforeSerialize() => CheckItemID();
        
        private void CheckItemID()
        {
            if (string.IsNullOrEmpty(itemID))
            {
                itemID = Guid.NewGuid().ToString();
            }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            // Require by the ISerializationCallbackReceiver but we don't need
            // to do anything with it.
        }
    }
}