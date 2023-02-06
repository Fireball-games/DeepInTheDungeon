using System.Collections.Generic;
using System.Linq;
using Scripts.Building.PrefabsSpawning.Configurations;
using Scripts.Building.Walls;
using Scripts.EventsManagement;
using Scripts.System;
using UnityEngine;

namespace Scripts.Building.PrefabsBuilding
{
    public abstract class PrefabServiceBase<TC, TPrefab> where TC : PrefabConfiguration where TPrefab : PrefabBase
    {
        protected static MapBuilder MapBuilder => GameManager.Instance.MapBuilder;
        protected static bool IsInEditMode => GameManager.Instance.GameMode == GameManager.EGameMode.Editor;
        
        public static Dictionary<string, PrefabStoreItem<TC, TPrefab>> Store;
        
        static PrefabServiceBase()
        {
            Store = new Dictionary<string, PrefabStoreItem<TC, TPrefab>>();
            
            EventsManager.OnMapDemolished += () => { Store.Clear(); };
        }
        
        protected abstract TC GetConfigurationFromPrefab(PrefabBase prefab, string ownerGuid, bool spawnPrefabOnBuild);
        
        public static void AddToStore(TC configuration, TPrefab prefabScript, GameObject prefab)
        {
            if (!IsInEditMode) return;
            
            Store.TryAdd(configuration.Guid, new PrefabStoreItem<TC, TPrefab>(configuration, prefabScript, prefab));
        }

        public static void RemoveFromStore(string guid)
        {
            if (!IsInEditMode) return;
            
            Store.Remove(guid);
        }

        public static TC GetConfiguration(string guid) => Store.TryGetValue(guid, out PrefabStoreItem<TC, TPrefab> item) ? item.Configuration : null;
        public static IEnumerable<TC> Configurations => Store.Values.Select(configuration => configuration.Configuration);
        public static IEnumerable<TPrefab> PrefabScripts => Store.Values.Select(prefabScript => prefabScript.PrefabScript);
        
        protected TC AddConfigurationToMap(TPrefab prefab, string ownerGuid)
        {
            if (MapBuilder.GetConfigurationByOwnerGuidAndName(ownerGuid, prefab.gameObject.name, out TriggerConfiguration configuration))
            {
                // Logger.Log("Configuration is already present.");
                return configuration as TC;
            }

            TC newConfiguration = GetConfigurationFromPrefab(prefab, ownerGuid, false);
            MapBuilder.AddReplacePrefabConfiguration(newConfiguration);
            return newConfiguration;
        }
    }
}