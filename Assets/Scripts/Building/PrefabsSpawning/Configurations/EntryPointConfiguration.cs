using UnityEngine;

namespace Scripts.Building.PrefabsSpawning.Configurations
{
    public class EntryPointConfiguration : PrefabConfiguration
    {
        public bool IsMovingForwardOnStart;
        public string EntryPointName;
        public Quaternion LookDirection;

        public EntryPointConfiguration()
        {
        }

        public EntryPointConfiguration(EntryPointConfiguration configuration) : base(configuration)
        {
            IsMovingForwardOnStart = configuration.IsMovingForwardOnStart;
            EntryPointName = configuration.EntryPointName;
            LookDirection = configuration.LookDirection;
        }
        
        public EntryPointConfiguration(EntryPointPrefab prefabScript, string ownerGuid, bool spawnPrefabOnBuild) : base(prefabScript, ownerGuid, spawnPrefabOnBuild)
        {
            IsMovingForwardOnStart = prefabScript.isMovingForwardOnStart;
            EntryPointName = prefabScript.entryPointName;
            LookDirection = prefabScript.lookDirection;
        }
    }
}