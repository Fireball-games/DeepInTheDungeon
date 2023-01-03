using System;
using Scripts.Building.Walls;
using Scripts.Helpers.Extensions;
using Scripts.System;
using static Scripts.Enums;

namespace Scripts.Building.PrefabsSpawning.Configurations
{
    public class PrefabConfiguration
    {
        public EPrefabType PrefabType;
        public string PrefabName;
        public PositionRotation TransformData;
        public string Guid;
        public bool SpawnPrefabOnBuild = true;

        protected PrefabConfiguration()
        {
        }

        protected PrefabConfiguration(PrefabConfiguration other)
        {
            PrefabName = other.PrefabName;
            TransformData = new PositionRotation(other.TransformData.Position, other.TransformData.Rotation);
            PrefabType = other.PrefabType;
            Guid = string.IsNullOrEmpty(other.Guid) ? global::System.Guid.NewGuid().ToString() : other.Guid;
            SpawnPrefabOnBuild = other.SpawnPrefabOnBuild;
        }

        protected PrefabConfiguration(PrefabBase prefab, bool spawnPrefabOnBuild = true)
        {
            PrefabName = prefab.name;
            TransformData = new PositionRotation(prefab.transform.position.Round(2), prefab.transform.rotation);
            PrefabType = prefab.prefabType;
            Guid = string.IsNullOrEmpty(prefab.GUID) ? global::System.Guid.NewGuid().ToString() : prefab.GUID;
            SpawnPrefabOnBuild = spawnPrefabOnBuild;
        }

        protected bool Equals(PrefabConfiguration other)
        {
            // return PrefabType == other.PrefabType && PrefabName == other.PrefabName && Equals(TransformData, other.TransformData);
            return Guid == other.Guid;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PrefabConfiguration)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine((int)PrefabType, PrefabName, TransformData);
        }
    }
}