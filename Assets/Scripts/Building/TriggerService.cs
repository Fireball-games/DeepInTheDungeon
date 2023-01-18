using System.Collections.Generic;
using System.Linq;
using Scripts.Building.PrefabsSpawning.Configurations;
using Scripts.Building.Walls;
using Scripts.MapEditor.Services;
using Scripts.System;
using Scripts.Triggers;
using UnityEngine;
using Logger = Scripts.Helpers.Logger;
using NotImplementedException = System.NotImplementedException;

namespace Scripts.Building
{
    public class TriggerService
    {
        private static bool IsInEditor => GameManager.Instance.GameMode == GameManager.EGameMode.Editor;

        private readonly PrefabBuilder _prefabBuilder;
        private readonly Dictionary<string, Trigger> _triggers;
        private readonly Dictionary<string, TriggerReceiver> _triggerReceivers;
        
        public TriggerService(PrefabBuilder prefabBuilder)
        {
            _prefabBuilder = prefabBuilder;
            _triggers = new Dictionary<string, Trigger>();
            _triggerReceivers = new Dictionary<string, TriggerReceiver>();
        }
        
        /// <summary>
        /// This method works with embedded triggers only and does:
        /// - subscribes new prefab to its triggers
        /// - in editor - registers triggers/triggerReceivers into their lists
        /// - in editor - creates trigger/triggerReceiver configurations from new prefab and ads those into map prefabConfigurations
        /// </summary>
        /// <param name="newPrefab"></param>
        internal void ProcessTriggersOnPrefab(GameObject newPrefab)
        {
            PrefabBase prefabScript = newPrefab.GetComponent<PrefabBase>();

            if (!prefabScript) return;

            foreach (Trigger trigger in newPrefab.GetComponentsInChildren<Trigger>().Where(c => c != prefabScript))
            {
                TriggerConfiguration configuration;

                if (IsInEditor)
                {
                    _triggers.TryAdd(trigger.GUID, trigger);

                    configuration = AddTriggerConfigurationToMap(trigger, prefabScript.GUID);
                }
                else
                {
                    if (!_prefabBuilder.GetConfigurationByOwnerGuidAndName(prefabScript.GUID, trigger.gameObject.name, out configuration))
                    {
                        Logger.LogWarning("Failed to find configuration for trigger", logObject: trigger);
                        continue;
                    }

                    trigger.subscribers = configuration.Subscribers;
                }

                trigger.GUID = configuration.Guid;
                
                if (trigger is IPositionsTrigger positionsTrigger)
                {
                    positionsTrigger.SetPosition();
                }
            }

            foreach (TriggerReceiver receiver in newPrefab.GetComponents<TriggerReceiver>())
            {
                TriggerReceiverConfiguration configuration;

                if (IsInEditor)
                {
                    _triggerReceivers.TryAdd(receiver.Guid, receiver);

                    configuration = AddTriggerReceiverConfigurationToMap(receiver, prefabScript.GUID);
                }
                else
                {
                    if (!_prefabBuilder.GetConfigurationByOwnerGuidAndName(prefabScript.GUID, receiver.identification, out configuration))
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
        
        private TriggerConfiguration AddTriggerConfigurationToMap(Trigger trigger, string ownerGuid)
        {
            if (_prefabBuilder.GetConfigurationByOwnerGuidAndName(ownerGuid, trigger.name, out TriggerConfiguration configuration))
            {
                // Logger.Log("Configuration is already present.");
                return configuration;
            }

            TriggerConfiguration newConfiguration = new(trigger, ownerGuid, false);
            _prefabBuilder.AddReplacePrefabConfiguration(newConfiguration);
            return newConfiguration;
        }

        private TriggerReceiverConfiguration AddTriggerReceiverConfigurationToMap(TriggerReceiver triggerReceiver, string ownerGuid)
        {
            if (_prefabBuilder.GetConfigurationByOwnerGuidAndName(ownerGuid, triggerReceiver.identification, out TriggerReceiverConfiguration configuration))
            {
                // Logger.Log("Configuration is already present.");
                return configuration;
            }

            TriggerReceiverConfiguration newConfiguration = new(triggerReceiver, ownerGuid, false);
            _prefabBuilder.AddReplacePrefabConfiguration(newConfiguration);
            return newConfiguration;
        }
        
        internal void RemoveEmbeddedTriggers(GameObject prefab)
        {
            PrefabBase prefabScript = prefab.GetComponent<PrefabBase>();

            if (!prefabScript) return;

            TriggerReceiver[] triggerReceivers = prefab.GetComponents<TriggerReceiver>();

            foreach (TriggerReceiver receiver in triggerReceivers)
            {
                foreach (TriggerConfiguration triggerConfiguration in _prefabBuilder.GetConfigurations<TriggerConfiguration>(Enums.EPrefabType.Trigger))
                {
                    if (triggerConfiguration.Subscribers.Contains(receiver.Guid))
                    {
                        triggerConfiguration.Subscribers.Remove(receiver.Guid);
                        PathsService.AddReplaceTriggerPath(triggerConfiguration);
                    }
                }

                _prefabBuilder.RemoveConfiguration(receiver.Guid);
            }

            foreach (Trigger trigger in prefab.GetComponentsInChildren<Trigger>())
            {
                _prefabBuilder.RemoveConfiguration(trigger.GUID);
            }
        }
        
        internal void ProcessTriggerConfiguration(PrefabConfiguration configuration, GameObject newPrefab)
        {
            if (configuration is not TriggerConfiguration triggerConfiguration) return;
            
            Trigger prefabScript = newPrefab.GetComponent<Trigger>();

            if (prefabScript)
            {
                prefabScript.triggerType = triggerConfiguration.TriggerType;
                prefabScript.count = triggerConfiguration.Count;
                prefabScript.subscribers = triggerConfiguration.Subscribers;
                if (prefabScript is IPositionsTrigger positionsTrigger)
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