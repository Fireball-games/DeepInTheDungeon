using System;
using System.Collections.Generic;
using System.Linq;
using Scripts.System;
using Unity.VisualScripting;

namespace Scripts.Building.ItemSpawning
{
    public struct MapObjectConfiguration : ICloneable
    {
        public string ID;
        public PositionRotation PositionRotation;
        public Dictionary<string, object> CustomData;

        public enum ECustomDataKey
        {
            Health = 0,
            StackSize = 1,
        }

        public object Clone() =>
            new MapObjectConfiguration
            {
                ID = ID,
                PositionRotation = PositionRotation,
                CustomData = CustomData?.ToDictionary(kv => kv.Key, kv => kv.Value),
            };
    }
}