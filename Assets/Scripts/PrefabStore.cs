using System.Collections.Generic;
using System.Linq;
using Scripts.Helpers;
using UnityEngine;
using static Scripts.Enums;

namespace Scripts
{
    public static class PrefabStore
    {
        private static readonly Dictionary<EPrefabType, HashSet<GameObject>> StoreMap;

        static PrefabStore()
        {
            StoreMap = new Dictionary<EPrefabType, HashSet<GameObject>>
            {
                {EPrefabType.Wall, new HashSet<GameObject>()}
            };
        }

        public static HashSet<GameObject> GetPrefabsOfType(EPrefabType prefabType)
        {
            if (StoreMap[prefabType].Any()) return StoreMap[prefabType];
            
            if(FileOperationsHelper.LoadPrefabs(prefabType, out HashSet<GameObject> loadedPrefabs))
            {
                StoreMap[prefabType] = loadedPrefabs;
            }

            return null;
        }
    }
}