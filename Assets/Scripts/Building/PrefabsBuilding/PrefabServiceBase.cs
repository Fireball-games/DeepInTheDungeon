using System.Collections.Generic;
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

        private static void AddToStore(TC configuration, TPrefab prefabScript, GameObject prefab)
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
        protected TPrefab GetPrefabScript(string guid) => Store.TryGetValue(guid, out PrefabStoreItem<TC, TPrefab> item) ? item.PrefabScript : null;
        protected static IEnumerable<TC> Configurations => Store.Values.Select(configuration => configuration.Configuration);
        protected static IEnumerable<TPrefab> PrefabScripts => Store.Values.Select(prefabScript => prefabScript.PrefabScript);

        private TC AddConfigurationToMap(TPrefab prefab, string ownerGuid)
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

        public void ProcessConfigurationOnBuild(PrefabConfiguration configuration, PrefabBase prefabScript, GameObject newPrefab)
        {
            if (configuration is not TC prefabConfiguration || prefabScript is not TPrefab script) return;
            
            newPrefab.transform.localRotation = configuration.TransformData.Rotation;
            
            ProcessConfiguration(prefabConfiguration, script, newPrefab);

            if (IsInEditMode)
            {
                AddToStore(prefabConfiguration, script, newPrefab);
            }
        }
        
        public void ProcessAllEmbedded(GameObject newPrefab)
        {
            PrefabBase prefabScript = newPrefab.GetComponent<PrefabBase>();

            if (!prefabScript) return;
            
            foreach (TPrefab embeddedPrefab in newPrefab.GetComponentsInChildren<TPrefab>())
            {
                TC configuration;
                // We dont want to process only embedded walls, not owners
                if (prefabScript.Guid == embeddedPrefab.Guid) continue;

                if (IsInEditMode)
                {
                    configuration = AddConfigurationToMap(embeddedPrefab, prefabScript.Guid);
                    AddToStore(configuration, embeddedPrefab, embeddedPrefab.gameObject);
                }
                else
                {
                    if (!MapBuilder.GetConfigurationByOwnerGuidAndName(prefabScript.Guid, embeddedPrefab.gameObject.name,
                            out configuration))
                    {
                        Helpers.Logger.LogWarning($"Failed to find configuration for {embeddedPrefab.gameObject.name}.", logObject: embeddedPrefab);
                        continue;
                    }
                    
                    ProcessEmbeddedForPlayMode(configuration, embeddedPrefab);
                }
                if (prefabScript.presentedInEditor) prefabScript.presentedInEditor.SetActive(IsInEditMode);
                embeddedPrefab.Guid = configuration.Guid;
                
                ProcessEmbedded(configuration, embeddedPrefab);
            }
        }

        public void Remove(PrefabConfiguration rawConfiguration)
        {
            if (rawConfiguration is not TC configuration) return;
            
            RemoveConfiguration(configuration);
            
            if (IsInEditMode)
            {
                RemoveFromStore(configuration.Guid);
            }
        }

        public virtual void RemoveAllEmbedded(GameObject prefabGo)
        {
            foreach (TPrefab embeddedPrefab in prefabGo.GetComponentsInChildren<TPrefab>())
            {
                RemoveEmbedded(embeddedPrefab);

                RemoveFromStore(embeddedPrefab.Guid);
                MapBuilder.RemoveConfiguration(embeddedPrefab.Guid);
            }
        }
        
        protected abstract void RemoveConfiguration(TC configuration);
        protected abstract void ProcessConfiguration(TC configuration, TPrefab prefabScript, GameObject newPrefab);
        /// <summary>
        /// This method runs on build non-embedded prefab for every embedded prefab of given type in the script.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="prefabScript"></param>
        protected abstract void ProcessEmbedded(TC configuration, TPrefab prefabScript);

        /// <summary>
        /// Optional method to process embedded prefab when needed to set stuff for play mode directly on the prefab.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="prefabScript"></param>
        protected virtual void ProcessEmbeddedForPlayMode(TC configuration, TPrefab prefabScript) { }

        /// <summary>
        /// This method runs on Remove for every embedded prefab of given type on the object.
        /// </summary>
        /// <param name="prefabScript"></param>
        protected abstract void RemoveEmbedded(TPrefab prefabScript);
        public IEnumerable<TC> GetConfigurations() => Configurations;
    }
}