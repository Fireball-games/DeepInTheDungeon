using System.Collections.Generic;
using Scripts.Building.PrefabsSpawning;
using Scripts.Building.PrefabsSpawning.Configurations;
using UnityEngine;

namespace Scripts.Building.PrefabsBuilding
{
    public interface IPrefabService<TC> where TC : PrefabConfiguration
    {
        public void ProcessEmbeddedPrefabs(GameObject newPrefab);
        public void RemoveEmbeddedPrefabs(GameObject prefabGo);
        public IEnumerable<TC> GetConfigurations();
        public GameObject GetGameObject(string guid);
    }
}