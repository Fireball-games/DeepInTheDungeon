using System.Collections.Generic;
using System.Linq;
using Scripts.Building.PrefabsSpawning;
using Scripts.Building.PrefabsSpawning.Configurations;
using UnityEngine;
using Logger = Scripts.Helpers.Logger;

namespace Scripts.Building.PrefabsBuilding
{
    public class EntryPointService : PrefabServiceBase<EntryPointConfiguration, EntryPointPrefab>
    {
        protected override EntryPointConfiguration GetConfigurationFromPrefab(PrefabBase prefab, string ownerGuid,
            bool spawnPrefabOnBuild) => new((EntryPointPrefab) prefab, ownerGuid, spawnPrefabOnBuild);

        protected override void RemoveConfiguration(EntryPointConfiguration configuration)
        {
            // Nothing special is needed here.
        }

        protected override void ProcessConfiguration(EntryPointConfiguration configuration,
            EntryPointPrefab prefabScript, GameObject newPrefab)
        {
            if (!IsInEditMode)
            {
                newPrefab.SetActive(false);
                return;
            }
            
            Transform body = newPrefab.transform.Find("Body");
            body.localRotation = Quaternion.Euler(0f, configuration.PlayerRotationY, 0f);
        }

        protected override void ProcessEmbedded(EntryPointConfiguration configuration, EntryPointPrefab prefabScript)
        {
            Logger.LogNotImplemented();
        }

        protected override void RemoveEmbedded(EntryPointPrefab prefabScript)
        {
            Logger.LogNotImplemented();
        }

        public static List<EntryPoint> ConvertEntryPointConfigurationsToEntryPoints() 
            => Configurations.Select(c => new EntryPoint(c)).ToList();
    }
}