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
        protected override TriggerConfiguration GetConfigurationFromPrefab(PrefabBase prefab, string ownerGuid,
            bool spawnPrefabOnBuild)
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
        /// <param name="configuration"></param>
        /// <param name="prefabScript"></param>
        protected override void ProcessEmbedded(TriggerConfiguration configuration, Trigger prefabScript)
        {
            if (IsInEditMode)
            {
                AddReplaceTriggerPath(configuration);
            }

            if (prefabScript is IPositionsTrigger positionsTrigger)
            {
                positionsTrigger.SetStartPosition(configuration.StartPosition);
                positionsTrigger.SetPosition();
            }
        }

        protected override void ProcessEmbeddedForPlayMode(TriggerConfiguration configuration, Trigger prefabScript)
        {
            prefabScript.subscribers = configuration.Subscribers;
        }

        protected override void RemoveEmbedded(Trigger prefabScript)
        {
            // Not used in triggers because they need special treatment. So whole RemoveAllEmbedded is overridden.
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
                    if (!MapBuilder.GetConfigurationByOwnerGuidAndName(prefabScript.Guid, receiver.identification,
                            out configuration))
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

        protected override void RemoveConfiguration(TriggerConfiguration configuration)
        {
            DestroyPath(EPathsType.Trigger, configuration.Guid);
        }

        private static TriggerReceiverConfiguration AddTriggerReceiverConfigurationToMap(
            TriggerReceiver triggerReceiver, string ownerGuid)
        {
            if (MapBuilder.GetConfigurationByOwnerGuidAndName(ownerGuid, triggerReceiver.identification,
                    out TriggerReceiverConfiguration configuration))
            {
                // Logger.Log("Configuration is already present.");
                return configuration;
            }

            TriggerReceiverConfiguration newConfiguration = new(triggerReceiver, ownerGuid, false);
            MapBuilder.AddReplacePrefabConfiguration(newConfiguration);
            return newConfiguration;
        }

        public override void RemoveAllEmbedded(GameObject prefab)
        {
            PrefabBase prefabScript = prefab.GetComponent<PrefabBase>();

            if (!prefabScript) return;

            // Remove trigger receivers
            foreach (TriggerReceiver receiver in prefab.GetComponents<TriggerReceiver>())
            {
                foreach (TriggerConfiguration triggerConfiguration in Configurations)
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

            // Remove triggers
            foreach (Trigger trigger in prefab.GetComponentsInChildren<Trigger>())
            {
                RemoveFromStore(trigger.Guid);
                MapBuilder.RemoveConfiguration(trigger.Guid);
            }
        }

        protected override void ProcessConfiguration(TriggerConfiguration configuration, Trigger script,
            GameObject newPrefab)
        {
            script.triggerType = configuration.TriggerType;
            script.count = configuration.Count;
            script.subscribers = configuration.Subscribers;

            if (script is IPositionsTrigger positionsTrigger)
            {
                positionsTrigger.SetStartPosition(configuration.StartPosition);
                positionsTrigger.SetPosition();
            }

            if (IsInEditMode)
            {
                AddReplaceTriggerPath(configuration);
            }
        }
    }
}