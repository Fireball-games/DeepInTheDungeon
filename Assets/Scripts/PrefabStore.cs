﻿using System.Collections.Generic;
using System.Linq;
using Scripts.Building.PrefabsSpawning;
using Scripts.Helpers;
using Scripts.System.Pooling;
using UnityEngine;
using static Scripts.Enums;

namespace Scripts
{
    public static class PrefabStore
    {
        private static readonly Dictionary<EPrefabType, HashSet<GameObject>> StoreMap;
        private static readonly Dictionary<string, GameObject> PrefabMap;

        private static readonly HashSet<EPrefabType> PrefabTypes = new()
        {
            EPrefabType.Wall, EPrefabType.Enemy, EPrefabType.Item, EPrefabType.Prop, EPrefabType.PrefabTile,
            EPrefabType.WallBetween, EPrefabType.WallForMovement, EPrefabType.WallOnWall, EPrefabType.Trigger
        };

        static PrefabStore()
        {
            StoreMap = new Dictionary<EPrefabType, HashSet<GameObject>>
            {
                {EPrefabType.Wall, new HashSet<GameObject>()}
            };

            PrefabMap = new Dictionary<string, GameObject>();

            LoadAllPrefabs();
        }

        public static HashSet<GameObject> GetPrefabsOfType(EPrefabType prefabType)
        {
            return StoreMap[prefabType];
        }

        public static GameObject Instantiate(string configurationPrefabName, GameObject parent)
        {
            return PrefabMap.ContainsKey(configurationPrefabName)
                ? ObjectPool.Instance.GetFromPool(PrefabMap[configurationPrefabName], parent)
                : null;
        }
        
        public static TP GetPrefabByName<TP>(string prefabName) where TP : PrefabBase 
            => !PrefabMap.TryGetValue(prefabName, out GameObject foundPrefab) ? null : foundPrefab.GetComponent<TP>();

        private static void LoadAllPrefabs()
        {
            foreach (EPrefabType prefabType in PrefabTypes)
            {
                if (!FileOperationsHelper.LoadPrefabs(prefabType, out HashSet<GameObject> loadedPrefabs))
                {
                    // TODO: Uncomment once all prefab types have some prefab
                    // Logger.LogWarning($"No prefabs found for type \"{prefabType}\".");
                }
            
                foreach (GameObject gameObject in loadedPrefabs.Where(gameObject => !PrefabMap.ContainsKey(gameObject.name)))
                {
                    PrefabMap.Add(gameObject.name, gameObject);
                }
            
                StoreMap[prefabType] = loadedPrefabs;
            }
        }
    }
}