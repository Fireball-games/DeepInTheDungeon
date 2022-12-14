using System.Collections.Generic;
using System.Linq;
using Scripts.Helpers;
using Scripts.System.Pooling;
using UnityEngine;
using static Scripts.Enums;

namespace Scripts
{
    public static class PrefabStore
    {
        private static readonly Dictionary<EPrefabType, HashSet<GameObject>> StoreMap;
        private static Dictionary<string, GameObject> _prefabMap;

        static PrefabStore()
        {
            StoreMap = new Dictionary<EPrefabType, HashSet<GameObject>>
            {
                {EPrefabType.Wall, new HashSet<GameObject>()}
            };

            _prefabMap = new Dictionary<string, GameObject>();
        }

        public static HashSet<GameObject> GetPrefabsOfType(EPrefabType prefabType)
        {
            if (StoreMap[prefabType].Any()) return StoreMap[prefabType];

            if (!FileOperationsHelper.LoadPrefabs(prefabType, out HashSet<GameObject> loadedPrefabs)) return null;

            foreach (GameObject gameObject in loadedPrefabs.Where(gameObject => !_prefabMap.ContainsKey(gameObject.name)))
            {
                _prefabMap.Add(gameObject.name, gameObject);
            }
            
            StoreMap[prefabType] = loadedPrefabs;
            return StoreMap[prefabType];
        }

        public static GameObject Instantiate(string configurationPrefabName, GameObject parent)
        {
            return _prefabMap.ContainsKey(configurationPrefabName)
                ? ObjectPool.Instance.GetFromPool(_prefabMap[configurationPrefabName], parent)
                : null;
        }
    }
}