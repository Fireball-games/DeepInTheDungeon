﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Scripts.Building.PrefabsSpawning.Configurations;
using Scripts.Building.PrefabsSpawning.Walls;
using Scripts.Building.Tile;
using Scripts.Building.Walls;
using Scripts.Helpers.Extensions;
using Scripts.MapEditor;
using Scripts.ScriptableObjects;
using Scripts.System;
using Scripts.System.Pooling;
using Scripts.Triggers;
using Unity.VisualScripting;
using UnityEngine;
using static Scripts.MapEditor.Services.PathsService;
using Logger = Scripts.Helpers.Logger;

namespace Scripts.Building
{
    public class PrefabBuilder
    {
        private static MapBuilder MapBuilder => GameManager.Instance.MapBuilder;
        private static MapDescription MapDescription => MapBuilder.MapDescription;
        private static HashSet<GameObject> Prefabs => MapBuilder.Prefabs;
        private static TileDescription[,,] Layout => MapBuilder.Layout;
        private static bool IsInEditor => GameManager.Instance.GameMode == GameManager.EGameMode.Editor;

        private Dictionary<string, Trigger> _triggers;
        private Dictionary<string, TriggerReceiver> _triggerReceivers;

        public PrefabBuilder()
        {
            _triggers = new Dictionary<string, Trigger>();
            _triggerReceivers = new Dictionary<string, TriggerReceiver>();
        }

        internal void BuildPrefabs(IEnumerable<PrefabConfiguration> configurations)
        {
            if (GameManager.Instance.GameMode == GameManager.EGameMode.Editor)
            {
                DestroyAllPaths();
            }

            MapBuilder.StartCoroutine(BuildPrefabsCoroutine(configurations));
        }

        private IEnumerator BuildPrefabsCoroutine(IEnumerable configurations)
        {
            foreach (PrefabConfiguration configuration in configurations.CloneViaSerialization())
            {
                BuildPrefab(configuration);

                yield return null;
            }
        }

        /// <summary>
        /// Builds new prefab and both stores configuration in MapDescription and GameObject in Prefabs list.
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public bool BuildPrefab(PrefabConfiguration configuration)
        {
            MapDescription.PrefabConfigurations ??= new List<PrefabConfiguration>();

            if (!MapDescription.PrefabConfigurations.Contains(configuration))
            {
                AddReplacePrefabConfiguration(configuration);
            }

            if (configuration.SpawnPrefabOnBuild)
            {
                GameObject newPrefab = BuildPhysicalPrefab(configuration);

                ProcessTriggersOnPrefab(newPrefab);

                bool isEditorMode = GameManager.Instance.GameMode is GameManager.EGameMode.Editor;

                if (isEditorMode && -newPrefab.transform.position.y < MapEditorManager.Instance.CurrentFloor)
                {
                    newPrefab.SetActive(false);
                }
            }

            return true;
        }

        public void RemovePrefab(PrefabConfiguration configuration)
        {
            if (configuration == null) return;

            PrefabConfiguration config = GetConfigurationByGuid<PrefabConfiguration>(configuration.Guid);

            if (config == null) return;

            RemoveConfiguration(config);

            GameObject prefabGo = Prefabs.FirstOrDefault(go =>
                go.name == configuration.PrefabName && go.transform.position == configuration.TransformData.Position);

            if (!prefabGo)
            {
                Logger.LogWarning($"No prefab of name \"{configuration.PrefabName}\" found for removal in Prefabs.");
                return;
            }

            Prefabs.Remove(prefabGo);

            if (configuration is TilePrefabConfiguration)
            {
                Layout.ByGridV3Int(prefabGo.transform.position.ToGridPosition()).IsForMovement = true;
            }

            if (configuration is WallConfiguration {WayPoints: { }} wall && wall.WayPoints.Any())
            {
                DestroyPath(EPathsType.Waypoint, wall.WayPoints);
            }

            RemoveEmbeddedTriggers(prefabGo);

            prefabGo.transform.rotation = Quaternion.Euler(Vector3.zero);
            Transform offsetTransform = prefabGo.GetBody();
            if (offsetTransform) offsetTransform.localPosition = Vector3.zero;

            ObjectPool.Instance.ReturnToPool(prefabGo);
        }

        public GameObject GetPrefabByConfiguration(PrefabConfiguration configuration)
        {
            GameObject result = Prefabs.FirstOrDefault(p => p.transform.position == configuration.TransformData.Position
                                                            && p.name == configuration.PrefabName);

            return !result ? null : result;
        }

        public GameObject GetPrefabByGridPosition(Vector3Int position)
        {
            GameObject result = Prefabs.FirstOrDefault(p => p.transform.position == position.ToWorldPositionV3Int());

            return !result ? null : result;
        }

        public GameObject GetPrefabByWorldPosition(Vector3Int position)
        {
            GameObject result = Prefabs.FirstOrDefault(p => p.transform.position == position);

            return !result ? null : result;
        }

        public IEnumerable<T> GetPrefabConfigurationsOnWorldPosition<T>(Vector3 worldPosition)
            where T : PrefabConfiguration
        {
            return MapDescription.PrefabConfigurations
                .Where(p => p.TransformData.Position == worldPosition && p is T)
                .Select(p => p as T);
        }

        public void AddReplacePrefabConfiguration(PrefabConfiguration newConfiguration)
        {
            int replaceIndex = FindIndexOfConfiguration(newConfiguration);

            if (replaceIndex != -1)
            {
                // Logger.Log("replacing configuration");
                MapDescription.PrefabConfigurations[replaceIndex] = newConfiguration;
            }
            else
            {
                MapDescription.PrefabConfigurations.Add(newConfiguration);
            }
        }

        public void ChangePrefabPositionsBy(Vector3 positionChangeDelta)
        {
            foreach (PrefabConfiguration configuration in MapDescription.PrefabConfigurations)
            {
                configuration.TransformData.Position += positionChangeDelta;

                if (configuration is not WallConfiguration wall) continue;

                if (wall.WayPoints == null || !wall.WayPoints.Any()) continue;

                DestroyPath(EPathsType.Waypoint, wall.WayPoints);

                foreach (Waypoint waypoint in wall.WayPoints)
                {
                    waypoint.position += positionChangeDelta;
                }
            }
        }

        public PrefabConfiguration GetPrefabConfigurationByTransformData(PositionRotation transformData)
        {
            return MapDescription.PrefabConfigurations.FirstOrDefault(c => c.TransformData.Equals(transformData));
        }

        private int FindIndexOfConfiguration(PrefabConfiguration configuration) =>
            MapDescription.PrefabConfigurations.FindIndex(c => c.Guid == configuration.Guid);

        private GameObject BuildPhysicalPrefab(PrefabConfiguration configuration)
        {
            GameObject newPrefab = PrefabStore.Instantiate(configuration.PrefabName, MapBuilder.PrefabsParent);

            if (!newPrefab)
            {
                Logger.LogError($"Prefab \"{configuration.PrefabName}\" was not found.");
                RemoveConfiguration(configuration);
                return null;
            }

            newPrefab.transform.position = configuration.TransformData.Position;

            PrefabBase prefabScript = newPrefab.GetComponent<PrefabBase>();

            prefabScript.GUID = configuration.Guid;

            ProcessTileConfiguration(configuration, newPrefab);
            ProcessWallConfiguration(configuration, prefabScript, newPrefab);

            MapBuilder.Prefabs.Add(newPrefab);

            return newPrefab;
        }

        private void ProcessTileConfiguration(PrefabConfiguration configuration, GameObject newPrefab)
        {
            if (configuration is TilePrefabConfiguration)
            {
                newPrefab.GetBody().rotation = configuration.TransformData.Rotation;
            }
        }

        private void ProcessWallConfiguration(PrefabConfiguration configuration, PrefabBase prefabScript, GameObject newPrefab)
        {
            if (configuration is WallConfiguration wallConfiguration)
            {
                newPrefab.transform.localRotation = configuration.TransformData.Rotation;

                Transform physicalPart = newPrefab.GetBody();

                if (physicalPart)
                {
                    Vector3 position = physicalPart.localPosition;
                    position.x += wallConfiguration.Offset;
                    physicalPart.localPosition = position;
                }

                if (GameManager.Instance.GameMode == GameManager.EGameMode.Editor)
                {
                    if (prefabScript is WallPrefabBase script)
                    {
                        if (script && script.presentedInEditor)
                        {
                            script.presentedInEditor.SetActive(true);
                        }

                        if (wallConfiguration.HasPath())
                        {
                            AddWaypointPath(EPathsType.Waypoint, wallConfiguration.WayPoints);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This method does:
        /// - subscribes new prefab to its triggers
        /// - in editor - registers triggers/triggerReceivers into their lists
        /// - in editor - creates trigger/triggerReceiver configurations from new prefab and ads those into map prefabConfigurations
        /// </summary>
        /// <param name="newPrefab"></param>
        private void ProcessTriggersOnPrefab(GameObject newPrefab)
        {
            PrefabBase prefabScript = newPrefab.GetComponent<PrefabBase>();

            if (!prefabScript) return;

            TriggerReceiver[] triggerReceivers = newPrefab.GetComponents<TriggerReceiver>();

            foreach (TriggerReceiver receiver in triggerReceivers)
            {
                receiver.PrefabGuid = prefabScript.GUID;
            }
            
            foreach (Trigger trigger in newPrefab.GetComponentsInChildren<Trigger>())
            {
                TriggerConfiguration configuration;
                
                if (IsInEditor)
                {
                    _triggers.TryAdd(trigger.GUID, trigger);

                    configuration = AddTriggerConfigurationToMap(trigger, prefabScript.GUID);
                }
                else
                {
                    if (!GetConfigurationByOwnerGuidAndName(prefabScript.GUID, trigger.gameObject.name, out configuration))
                    {
                        Logger.LogWarning("Failed to find configuration for trigger", logObject: trigger);
                        continue;
                    }
                    
                    trigger.subscribers = configuration.Subscribers;
                }
                
                trigger.GUID = configuration.Guid;
            }

            foreach (TriggerReceiver receiver in triggerReceivers)
            {
                TriggerReceiverConfiguration configuration;
                    
                if (IsInEditor)
                {
                    _triggerReceivers.TryAdd(receiver.Guid, receiver);

                    configuration = AddTriggerReceiverConfigurationToMap(receiver, prefabScript.GUID);
                }
                else
                {
                    if (!GetConfigurationByOwnerGuidAndName(prefabScript.GUID, receiver.identification, out configuration))
                    {
                        Logger.LogWarning("Failed to find configuration for trigger", logObject: receiver);
                        continue;
                    }
                    
                    receiver.startMovement = configuration.StartMovement;
                }
                
                receiver.Guid = configuration.Guid;
                receiver.SetMovementStep();
            }
        }

        private TriggerConfiguration AddTriggerConfigurationToMap(Trigger trigger, string ownerGuid)
        {
            if (GetConfigurationByOwnerGuidAndName(ownerGuid, trigger.name, out TriggerConfiguration configuration))
            {
                // Logger.Log("Configuration is already present.");
                return configuration;
            }
            
            TriggerConfiguration newConfiguration = new(trigger, ownerGuid, false);
            AddReplacePrefabConfiguration(newConfiguration);
            return newConfiguration;
        }
        
        private TriggerReceiverConfiguration AddTriggerReceiverConfigurationToMap(TriggerReceiver triggerReceiver, string ownerGuid)
        {
            if (GetConfigurationByOwnerGuidAndName(ownerGuid, triggerReceiver.identification, out TriggerReceiverConfiguration configuration))
            {
                // Logger.Log("Configuration is already present.");
                return configuration;
            }
            
            TriggerReceiverConfiguration newConfiguration = new(triggerReceiver, ownerGuid, false);
            AddReplacePrefabConfiguration(newConfiguration);
            return newConfiguration;
        }

        private void RemoveConfiguration(string guid)
        {
            RemoveConfiguration(GetConfigurationByGuid<PrefabConfiguration>(guid));
        }

        private void RemoveConfiguration(PrefabConfiguration configuration)
        {
            MapDescription.PrefabConfigurations.Remove(configuration);
        }
        
        private void RemoveEmbeddedTriggers(GameObject prefab)
        {
            PrefabBase prefabScript = prefab.GetComponent<PrefabBase>();

            if (!prefabScript) return;
            
            TriggerReceiver[] triggerReceivers = prefab.GetComponents<TriggerReceiver>();

            foreach (TriggerReceiver receiver in triggerReceivers)
            {
                foreach (TriggerConfiguration triggerConfiguration in GetAllPrefabsByType<TriggerConfiguration>(Enums.EPrefabType.Trigger))
                {
                    // TODO: Not tested, test once more triggerReceivers can be assigned to trigger
                    if (triggerConfiguration.Subscribers.Contains(receiver.PrefabGuid))
                    {
                        triggerConfiguration.Subscribers.Remove(receiver.PrefabGuid);
                    }
                }
                
                RemoveConfiguration(receiver.Guid);
            }

            foreach (Trigger trigger in prefab.GetComponentsInChildren<Trigger>())
            {
                
                RemoveConfiguration(trigger.GUID);
            }
        }
        
        private IEnumerable<T> GetAllPrefabsByType<T>(Enums.EPrefabType prefabType) where T : PrefabConfiguration
        {
            return MapDescription.PrefabConfigurations.Where(c => c.PrefabType == prefabType)
                .Select(c => c as T);
        }

        private T GetConfigurationByGuid<T>(string guid) where T : PrefabConfiguration
        {
            return MapDescription.PrefabConfigurations.Where(c => c.Guid == guid).FirstOrDefault() as T;
        }

        private bool GetConfigurationByOwnerGuidAndName<T>(string ownerGuid, string prefabName, out T configuration) where T : PrefabConfiguration
        {
            configuration = MapDescription.PrefabConfigurations
                .Where(c => c.OwnerGuid == ownerGuid && c.PrefabName == prefabName)
                .FirstOrDefault() as T;

            return configuration != null;
        }
    }
}