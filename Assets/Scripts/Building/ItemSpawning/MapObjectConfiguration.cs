using System;
using System.Collections.Generic;
using System.Linq;
using Scripts.Helpers.Extensions;
using Scripts.Inventory.Inventories;
using Scripts.Inventory.Inventories.Items;
using Scripts.System;
using UnityEngine;

namespace Scripts.Building.ItemSpawning
{
    public class MapObjectConfiguration : ICloneable
    {
        public string ID;
        public PositionRotation PositionRotation;
        public Dictionary<ECustomDataKey, object> CustomData;

        public enum ECustomDataKey
        {
            Health = 0,
            StackSize = 1,
        }

        public static MapObjectConfiguration Create(MapObjectInstance item)
        {
            MapObjectConfiguration configuration = new()
            {
                ID = item.ItemID,
                PositionRotation = new PositionRotation(item.transform.position, item.transform.rotation),
                CustomData = new Dictionary<ECustomDataKey, object>(),
            };

            if (item is Pickup pickup)
            {
                configuration.CustomData.Add(ECustomDataKey.StackSize, pickup.StackSize);
            }

            if (item.Item is DestroyableProp destroyableProp)
            {
                configuration.CustomData.Add(ECustomDataKey.Health, destroyableProp.Health);
            }

            return configuration;
        }

        public static MapObjectConfiguration Create(MapObject item) => new()
        {
            ID = item.GetItemID(),
            PositionRotation = new PositionRotation(V3Extensions.Zero, Quaternion.identity),
            CustomData = new Dictionary<ECustomDataKey, object>(),
        };

        public object Clone() =>
            new MapObjectConfiguration
            {
                ID = ID,
                PositionRotation = PositionRotation,
                CustomData = CustomData?.ToDictionary(kv => kv.Key, kv => kv.Value),
            };
    }
}