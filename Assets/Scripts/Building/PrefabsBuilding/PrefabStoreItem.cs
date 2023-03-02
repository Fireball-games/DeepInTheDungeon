using Scripts.Building.PrefabsSpawning.Configurations;
using UnityEngine;

namespace Scripts.Building.PrefabsBuilding
{
    public class PrefabStoreItem<TC, TPrefab> where TC: PrefabConfiguration
    {
        public TC Configuration;
        public TPrefab PrefabScript;
        public GameObject GameObject;

        public PrefabStoreItem(TC configuration, TPrefab prefabScript, GameObject gameObject)
        {
            Configuration = configuration;
            PrefabScript = prefabScript;
            GameObject = gameObject;
        }
    }
}