using System.Collections.Generic;
using System.Linq;
using Scripts.Building.PrefabsSpawning;
using Scripts.Building.PrefabsSpawning.Configurations;
using Scripts.EventsManagement;
using Scripts.Triggers;
using UnityEngine;
using static Scripts.MapEditor.Services.PathsService;
using Logger = Scripts.Helpers.Logger;

namespace Scripts.Building.PrefabsBuilding
{
    public class TriggerService : PrefabServiceBase<TriggerConfiguration, Trigger>
    {
        protected override TriggerConfiguration GetConfigurationFromPrefab(PrefabBase prefab, string ownerGuid, bool spawnPrefabOnBuild)
        {
            return new TriggerConfiguration(prefab as Trigger, ownerGuid, spawnPrefabOnBuild);
        }

        private static readonly Dictionary<string, TriggerReceiver> TriggerReceivers;
        
        static TriggerService()
        {
            TriggerReceivers = new Dictionary<string, TriggerReceiver>();
            
            EventsManager.OnMapDemolished += () => { TriggerReceivers.Clear(); };
        }
        
        /// <summary>
        /// This method works with embedded triggers only and does:
        /// - subscribes new prefab to its triggers
        /// - in editor - registers triggers/triggerReceivers into their lists
        /// - in editor - creates trigger/triggerReceiver configurations from new prefab and ads those into map prefabConfigurations
        /// </summary>
        /// <param name="newPrefab"></param>
        public override void ProcessEmbeddedPrefabs(GameObject newPrefab)
        {
            Trigger prefabScript = newPrefab.GetComponent<Trigger>();

            if (!prefabScript) return;

            foreach (Trigger trigger in newPrefab.GetComponentsInChildren<Trigger>().Where(c => c != prefabScript))
            {
                TriggerConfiguration configuration;

                if (IsInEditMode)
                {
                    configuration = AddConfigurationToMap(trigger, prefabScript.Guid);
                    AddToStore(configuration, prefabScript, newPrefab);
                    AddReplaceTriggerPath(configuration);
                }
                else
                {
                    if (!MapBuilder.GetConfigurationByOwnerGuidAndName(prefabScript.Guid, trigger.gameObject.name, out configuration))
                    {
                        Logger.LogWarning("Failed to find configuration for trigger", logObject: trigger);
                        continue;
                    }

                    trigger.subscribers = configuration.Subscribers;
                }

                trigger.Guid = configuration.Guid;
                
                if (trigger is IPositionsTrigger positionsTrigger)
                {
                    positionsTrigger.SetStartPosition(configuration.StartPosition);
                    positionsTrigger.SetPosition();
                }
            }
        }

        public static void ProcessEmbeddedTriggerReceivers(GameObject newPrefab)
        {
            PrefabBase prefabScript = newPrefab.GetComponent<PrefabBase>();

            if (!prefabScript) return;
            
            foreach (TriggerReceiver receiver in newPrefab.GetComponents<TriggerReceiver>())
            {
                TriggerReceiverConfiguration configuration;

                if (IsInEditMode)
                {
                    TriggerReceivers.TryAdd(receiver.Guid, receiver);

                    configuration = AddTriggerReceiverConfigurationToMap(receiver, prefabScript.Guid);
                }
                else
                {
                    if (!MapBuilder.GetConfigurationByOwnerGuidAndName(prefabScript.Guid, receiver.identification, out configuration))
                    {
                        Logger.LogWarning("Failed to find configuration for trigger", logObject: receiver);
                        continue;
                    }
                }

                receiver.startPosition = configuration.StartPosition;
                receiver.Guid = configuration.Guid;
                receiver.SetPosition();
            }
        }
        
        public static void Remove(PrefabConfiguration configuration)
        {
            RemoveFromStore(configuration.Guid);
            DestroyPath(EPathsType.Trigger, configuration.Guid);
        }

        private static TriggerReceiverConfiguration AddTriggerReceiverConfigurationToMap(TriggerReceiver triggerReceiver, string ownerGuid)
        {
            if (MapBuilder.GetConfigurationByOwnerGuidAndName(ownerGuid, triggerReceiver.identification, out TriggerReceiverConfiguration configuration))
            {
                // Logger.Log("Configuration is already present.");
                return configuration;
            }

            TriggerReceiverConfiguration newConfiguration = new(triggerReceiver, ownerGuid, false);
            MapBuilder.AddReplacePrefabConfiguration(newConfiguration);
            return newConfiguration;
        }
        
        public override void RemoveEmbeddedPrefabs(GameObject prefab)
        {
            PrefabBase prefabScript = prefab.GetComponent<PrefabBase>();

            if (!prefabScript) return;

            TriggerReceiver[] triggerReceivers = prefab.GetComponents<TriggerReceiver>();

            foreach (TriggerReceiver receiver in triggerReceivers)
            {
                foreach (TriggerConfiguration triggerConfiguration in MapBuilder.GetConfigurations<TriggerConfiguration>(Enums.EPrefabType.Trigger))
                {
                    if (triggerConfiguration.Subscribers.Contains(receiver.Guid))
                    {
                        triggerConfiguration.Subscribers.Remove(receiver.Guid);
                        AddReplaceTriggerPath(triggerConfiguration);
                    }
                }
                
                TriggerReceivers.Remove(receiver.Guid);
                MapBuilder.RemoveConfiguration(receiver.Guid);
            }

            foreach (Trigger trigger in prefab.GetComponentsInChildren<Trigger>())
            {
                RemoveFromStore(trigger.Guid);
                MapBuilder.RemoveConfiguration(trigger.Guid);
            }
        }
        
        internal static void ProcessConfigurationOnBuild(PrefabConfiguration configuration, PrefabBase prefabScript, GameObject newPrefab)
        {
            if (configuration is not TriggerConfiguration triggerConfiguration || prefabScript is not Trigger script) return;

            if (prefabScript)
            {
                script.triggerType = triggerConfiguration.TriggerType;
                script.count = triggerConfiguration.Count;
                script.subscribers = triggerConfiguration.Subscribers;
                
                if (script is IPositionsTrigger positionsTrigger)
                {
                    positionsTrigger.SetStartPosition(triggerConfiguration.StartPosition);
                    positionsTrigger.SetPosition();
                }
            }

            newPrefab.transform.localRotation = configuration.TransformData.Rotation;

            if (IsInEditMode)
            {
                AddToStore(triggerConfiguration, script, newPrefab);
                AddReplaceTriggerPath(triggerConfiguration);
            }
        }
    }
}