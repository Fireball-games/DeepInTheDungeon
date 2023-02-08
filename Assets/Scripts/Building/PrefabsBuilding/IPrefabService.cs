using System.Collections.Generic;
using Scripts.Building.PrefabsSpawning;
using Scripts.Building.PrefabsSpawning.Configurations;
using UnityEngine;

namespace Scripts.Building.PrefabsBuilding
{
    public interface IPrefabService<TC> where TC : PrefabConfiguration
    {
        public void ProcessConfigurationOnBuild(PrefabConfiguration configuration, PrefabBase prefabScript, GameObject newPrefab);
        public void Remove(PrefabConfiguration configuration);
        public void ProcessAllEmbedded(GameObject newPrefab);
        public void RemoveAllEmbedded(GameObject prefabGo);
        public IEnumerable<TC> GetConfigurations();
        public GameObject GetGameObject(string guid);
    }
}