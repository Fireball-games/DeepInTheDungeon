namespace Scripts.Building.PrefabsSpawning.Configurations
{
    public class TilePrefabConfiguration : PrefabConfiguration
    {
        public bool IsWalkable;

        public TilePrefabConfiguration()
        {
        }

        public TilePrefabConfiguration(TilePrefabConfiguration configuration) : base(configuration)
        {
            IsWalkable = configuration.IsWalkable;
        }
        
        public TilePrefabConfiguration(TilePrefab prefabScript, string ownerGuid, bool spawnPrefabOnBuild) : base(prefabScript, ownerGuid, spawnPrefabOnBuild)
        {
            IsWalkable = prefabScript.isWalkable;
        }
    }
}