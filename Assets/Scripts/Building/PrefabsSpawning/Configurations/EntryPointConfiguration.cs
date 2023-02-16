namespace Scripts.Building.PrefabsSpawning.Configurations
{
    public class EntryPointConfiguration : PrefabConfiguration
    {
        public bool IsMovingForwardOnStart;
        public string Name;
        public int PlayerRotationY;
        public override string DisplayName => Name;

        public EntryPointConfiguration()
        {
        }

        public EntryPointConfiguration(EntryPointConfiguration configuration) : base(configuration)
        {
            IsMovingForwardOnStart = configuration.IsMovingForwardOnStart;
            Name = configuration.Name;
            PlayerRotationY = configuration.PlayerRotationY;
        }
        
        public EntryPointConfiguration(EntryPointPrefab prefabScript, string ownerGuid, bool spawnPrefabOnBuild)
        {
            // Not valid really. EntryPoints are editor only and not physical prefabs in Play mode. 
        }
    }
}