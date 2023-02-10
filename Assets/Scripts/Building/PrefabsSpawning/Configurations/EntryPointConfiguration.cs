using UnityEngine;

namespace Scripts.Building.PrefabsSpawning.Configurations
{
    public class EntryPointConfiguration : PrefabConfiguration
    {
        public bool IsMovingForwardOnStart;
        public string EntryPointName;
        public Quaternion LookDirection;
        public override string DisplayName { get; }

        public EntryPointConfiguration()
        {
        }

        public EntryPointConfiguration(EntryPointConfiguration configuration) : base(configuration)
        {
            IsMovingForwardOnStart = configuration.IsMovingForwardOnStart;
            EntryPointName = configuration.EntryPointName;
            LookDirection = configuration.LookDirection;
            DisplayName = configuration.EntryPointName;
        }
        
        public EntryPointConfiguration(EntryPointPrefab prefabScript, string ownerGuid, bool spawnPrefabOnBuild)
        {
            // Not valid really. EntryPoints are editor only and not physical prefabs in Play mode. 
        }
    }
}