using Scripts.Building.Walls.Configurations;

namespace Scripts.Building.PrefabsSpawning.Configurations
{
    public class TilePrefabConfiguration : PrefabConfiguration
    {
        public bool IsWalkable;

        public TilePrefabConfiguration()
        {
        }

        public TilePrefabConfiguration(TilePrefabConfiguration configuration)
        {
            IsWalkable = configuration.IsWalkable;

            PrefabName = configuration.PrefabName;
            TransformData = configuration.TransformData;
            PrefabType = configuration.PrefabType;
        }
    }
}