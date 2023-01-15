using System.Collections;
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

        private readonly Dictionary<string, Trigger> _triggers;
        private readonly Dictionary<string, TriggerReceiver> _triggerReceivers;

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

                if (isEditorMode && Mathf.RoundToInt(-newPrefab.transform.position.y) < MapEditorManager.Instance.CurrentFloor)
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

            GameObject prefabGo = Prefabs.FirstOrDefault(go => go.GetComponent<PrefabBase>().GUID == configuration.Guid);

            if (!configuration.SpawnPrefabOnBuild) return;
            
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

        public GameObject GetPrefabByGuid(string guid)
        {
            GameObject result = Prefabs.FirstOrDefault(p => p.GetComponent<PrefabBase>().GUID == guid);

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

        public IEnumerable<TC> GetConfigurationsByPrefabClass<TC, TP>() where TP : PrefabBase where TC : PrefabConfiguration
        {
            List<TC> result = new();
            
            foreach (PrefabBase script in Prefabs.Select(go => go.GetComponent<PrefabBase>()))
            {
                if (script is TP @base) result.Add(GetConfigurationByGuid<TC>(@base.GUID));
            }

            return result;
        }
        
        public TC GetConfigurationByGuid<TC>(string guid) where TC : PrefabConfiguration
        {
            return MapDescription.PrefabConfigurations.Where(c => c.Guid == guid).FirstOrDefault() as TC;
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
            ProcessTriggerConfiguration(configuration, newPrefab);

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
        
        private void ProcessTriggerConfiguration(PrefabConfiguration configuration, GameObject newPrefab)
        {
            if (configuration is TriggerConfiguration triggerConfiguration)
            {
                Trigger prefabScript = newPrefab.GetComponent<Trigger>();
                
                if (prefabScript)
                {
                    prefabScript.triggerType = triggerConfiguration.TriggerType;
                    prefabScript.count = triggerConfiguration.Count;
                    prefabScript.subscribers = triggerConfiguration.Subscribers;
                    prefabScript.startPosition = triggerConfiguration.StartPosition;
                    prefabScript.SetMovementStep();
                }
                
                newPrefab.transform.localRotation = configuration.TransformData.Rotation;
            }
            
            // if (configuration is TriggerReceiverConfiguration receiverConfiguration)
            // {
            //     TriggerReceiver prefabScript = newPrefab.GetComponent<TriggerReceiver>();
            //     
            //     if (prefabScript)
            //     {
            //         prefabScript.startPosition = receiverConfiguration.StartPosition;
            //         prefabScript.SetPosition();
            //     }
            // }
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
        /// This method works with embedded triggers only and does:
        /// - subscribes new prefab to its triggers
        /// - in editor - registers triggers/triggerReceivers into their lists
        /// - in editor - creates trigger/triggerReceiver configurations from new prefab and ads those into map prefabConfigurations
        /// </summary>
        /// <param name="newPrefab"></param>
        private void ProcessTriggersOnPrefab(GameObject newPrefab)
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
                    if (!GetConfigurationByOwnerGuidAndName(prefabScript.GUID, trigger.gameObject.name, out configuration))
                    {
                        Logger.LogWarning("Failed to find configuration for trigger", logObject: trigger);
                        continue;
                    }
                    
                    trigger.subscribers = configuration.Subscribers;
                }
                
                trigger.GUID = configuration.Guid;
                trigger.SetMovementStep();
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
                    if (!GetConfigurationByOwnerGuidAndName(prefabScript.GUID, receiver.identification, out configuration))
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
                foreach (TriggerConfiguration triggerConfiguration in GetConfigurations<TriggerConfiguration>(Enums.EPrefabType.Trigger))
                {
                    if (triggerConfiguration.Subscribers.Contains(receiver.Guid))
                    {
                        triggerConfiguration.Subscribers.Remove(receiver.Guid);
                    }
                }
                
                RemoveConfiguration(receiver.Guid);
            }

            foreach (Trigger trigger in prefab.GetComponentsInChildren<Trigger>())
            {
                
                RemoveConfiguration(trigger.GUID);
            }
        }

        private bool GetConfigurationByOwnerGuidAndName<T>(string ownerGuid, string prefabName, out T configuration) where T : PrefabConfiguration
        {
            configuration = MapDescription.PrefabConfigurations
                .Where(c => c.OwnerGuid == ownerGuid && c.PrefabName == prefabName)
                .FirstOrDefault() as T;

            return configuration != null;
        }

        public IEnumerable<TC> GetConfigurations<TC>(Enums.EPrefabType ePrefabType) where TC : PrefabConfiguration =>
            MapDescription.PrefabConfigurations.Where(c => c.PrefabType == ePrefabType)
                .Select(c => c as TC);
    }
}