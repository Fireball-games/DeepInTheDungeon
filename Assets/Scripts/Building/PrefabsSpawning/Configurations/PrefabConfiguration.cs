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

        public string GUID
        {
            get {
            if (string.IsNullOrEmpty(_guid))
            {
                _guid = Guid.NewGuid().ToString();
            }

            return _guid;
            }

            internal set => _guid = value;
        }

        private string _guid;

        protected PrefabConfiguration()
        {
            _guid = Guid.NewGuid().ToString();
        }

        protected PrefabConfiguration(PrefabConfiguration other)
        {
            PrefabName = other.PrefabName;
            TransformData = new PositionRotation(other.TransformData.Position, other.TransformData.Rotation);
            PrefabType = other.PrefabType;
            _guid = other._guid;
        }
        
        protected bool Equals(PrefabConfiguration other)
        {
            // return PrefabType == other.PrefabType && PrefabName == other.PrefabName && Equals(TransformData, other.TransformData);
            return _guid == other._guid;
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