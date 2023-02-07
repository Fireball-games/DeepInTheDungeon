﻿using System.Collections.Generic;
using System.Linq;
using Scripts.Building.PrefabsSpawning;
using Scripts.Building.PrefabsSpawning.Configurations;
using Scripts.EventsManagement;
using Scripts.System;
using UnityEngine;

namespace Scripts.Building.PrefabsBuilding
{
    public abstract class PrefabServiceBase<TC, TPrefab> : IPrefabService<TC> 
        where TC : PrefabConfiguration 
        where TPrefab : PrefabBase
    {
        protected static MapBuilder MapBuilder => GameManager.Instance.MapBuilder;
        protected static bool IsInEditMode => GameManager.Instance.GameMode == GameManager.EGameMode.Editor;

        private static readonly Dictionary<string, PrefabStoreItem<TC, TPrefab>> Store;
        
        static PrefabServiceBase()
        {
            Store = new Dictionary<string, PrefabStoreItem<TC, TPrefab>>();
            
            EventsManager.OnMapDemolished += () => { Store.Clear(); };
        }
        
        protected abstract TC GetConfigurationFromPrefab(PrefabBase prefab, string ownerGuid, bool spawnPrefabOnBuild);

        protected static void AddToStore(TC configuration, TPrefab prefabScript, GameObject prefab)
        {
            if (!IsInEditMode) return;
            
            Store.TryAdd(configuration.Guid, new PrefabStoreItem<TC, TPrefab>(configuration, prefabScript, prefab));
        }

        protected static void RemoveFromStore(string guid)
        {
            if (!IsInEditMode) return;
            
            Store.Remove(guid);
        }

        public GameObject GetGameObject(string guid) => Store.TryGetValue(guid, out PrefabStoreItem<TC, TPrefab> item) ? item.GameObject : null;
        public static IEnumerable<TC> Configurations => Store.Values.Select(configuration => configuration.Configuration);
        protected static IEnumerable<TPrefab> PrefabScripts => Store.Values.Select(prefabScript => prefabScript.PrefabScript);
        
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

        public abstract void ProcessEmbeddedPrefabs(GameObject newPrefab);

        public abstract void RemoveEmbeddedPrefabs(GameObject prefabGo);

        public IEnumerable<TC> GetConfigurations() => Configurations;
    }
}