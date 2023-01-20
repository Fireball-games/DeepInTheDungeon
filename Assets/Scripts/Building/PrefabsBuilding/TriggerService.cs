using System.Collections.Generic;
using System.Linq;
using Scripts.Building.PrefabsSpawning.Configurations;
using Scripts.Building.Walls;
using Scripts.EventsManagement;
using Scripts.MapEditor.Services;
using Scripts.System;
using Scripts.Triggers;
using UnityEngine;
using Logger = Scripts.Helpers.Logger;

namespace Scripts.Building.PrefabsBuilding
{
    public static class TriggerService
    {
        private static MapBuilder MapBuilder => GameManager.Instance.MapBuilder;
        private static bool IsInEditor => GameManager.Instance.GameMode == GameManager.EGameMode.Editor;

        public static readonly Dictionary<string, Trigger> Triggers;
        public static readonly Dictionary<string, TriggerReceiver> TriggerReceivers;
        
        static TriggerService()
        {
            Triggers = new Dictionary<string, Trigger>();
            TriggerReceivers = new Dictionary<string, TriggerReceiver>();
            
            EventsManager.OnMapDemolished += () => { Triggers.Clear(); TriggerReceivers.Clear(); };
        }
        
        /// <summary>
        /// This method works with embedded triggers only and does:
        /// - subscribes new prefab to its triggers
        /// - in editor - registers triggers/triggerReceivers into their lists
        /// - in editor - creates trigger/triggerReceiver configurations from new prefab and ads those into map prefabConfigurations
        /// </summary>
        /// <param name="newPrefab"></param>
        internal static void ProcessEmbeddedTriggers(GameObject newPrefab)
        {
            PrefabBase prefabScript = newPrefab.GetComponent<PrefabBase>();

            if (!prefabScript) return;

            foreach (Trigger trigger in newPrefab.GetComponentsInChildren<Trigger>().Where(c => c != prefabScript))
            {
                TriggerConfiguration configuration;

                if (IsInEditor)
                {
                    Triggers.TryAdd(trigger.GUID, trigger);

                    configuration = AddTriggerConfigurationToMap(trigger, prefabScript.GUID);
                    PathsService.AddReplaceTriggerPath(configuration);
                }
                else
                {
                    if (!MapBuilder.GetConfigurationByOwnerGuidAndName(prefabScript.GUID, trigger.gameObject.name, out configuration))
                    {
                        Logger.LogWarning("Failed to find configuration for trigger", logObject: trigger);
                        continue;
                    }

                    trigger.subscribers = configuration.Subscribers;
                }

                trigger.GUID = configuration.Guid;
                
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

                if (IsInEditor)
                {
                    TriggerReceivers.TryAdd(receiver.Guid, receiver);

                    configuration = AddTriggerReceiverConfigurationToMap(receiver, prefabScript.GUID);
                }
                else
                {
                    if (!MapBuilder.GetConfigurationByOwnerGuidAndName(prefabScript.GUID, receiver.identification, out configuration))
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
        
        private static TriggerConfiguration AddTriggerConfigurationToMap(Trigger trigger, string ownerGuid)
        {
            if (MapBuilder.GetConfigurationByOwnerGuidAndName(ownerGuid, trigger.name, out TriggerConfiguration configuration))
            {
                // Logger.Log("Configuration is already present.");
                return configuration;
            }

            TriggerConfiguration newConfiguration = new(trigger, ownerGuid, false);
            MapBuilder.AddReplacePrefabConfiguration(newConfiguration);
            return newConfiguration;
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
        
        internal static void RemoveEmbeddedTriggers(GameObject prefab)
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
                        PathsService.AddReplaceTriggerPath(triggerConfiguration);
                    }
                }

                MapBuilder.RemoveConfiguration(receiver.Guid);
            }

            foreach (Trigger trigger in prefab.GetComponentsInChildren<Trigger>())
            {
                MapBuilder.RemoveConfiguration(trigger.GUID);
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

            if (IsInEditor)
            {
                PathsService.AddReplaceTriggerPath(triggerConfiguration);
            }
        }
    }
}