using System;
using Scripts.System;
using static Scripts.Enums;

namespace Scripts.Building.PrefabsSpawning.Configurations
{
    public class PrefabConfiguration
    {
        public EPrefabType PrefabType;
        public string PrefabName;
        public PositionRotation TransformData;

        protected bool Equals(PrefabConfiguration other)
        {
            return PrefabType == other.PrefabType && PrefabName == other.PrefabName && Equals(TransformData, other.TransformData);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PrefabConfiguration) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine((int) PrefabType, PrefabName, TransformData);
        }
    }
}