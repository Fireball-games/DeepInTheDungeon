using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Scripts.Building.PrefabsSpawning;
using Scripts.Building.PrefabsSpawning.Configurations;
using Scripts.Helpers.Extensions;
using Scripts.MapEditor;
using Scripts.ScriptableObjects;
using Scripts.System;
using Scripts.System.Pooling;
using Scripts.Triggers;
using Unity.VisualScripting;
using UnityEngine;
using static Scripts.Enums;
using static Scripts.MapEditor.Services.PathsService;
using Logger = Scripts.Helpers.Logger;

namespace Scripts.Building.PrefabsBuilding
{
    public class PrefabBuilder
    {
        private readonly Dictionary<string, Trigger> _triggers;
        private readonly Dictionary<string, TriggerReceiver> _triggerReceivers;
        private static MapBuilder MapBuilder => GameManager.Instance.MapBuilder;
        private static MapDescription MapDescription => MapBuilder.MapDescription;
        private static HashSet<GameObject> Prefabs => MapBuilder.Prefabs;

        private readonly WallService _wallService;
        private readonly TriggerService _triggerService;
        private readonly TilePrefabService _tilePrefabService;
        private readonly EntryPointService _entryPointService;
        
        public PrefabBuilder()
        {
            _wallService = new WallService();
            _triggerService = new TriggerService();
            _tilePrefabService = new TilePrefabService();
            _entryPointService = new EntryPointService();
        }

        internal IEnumerator BuildPrefabs(IEnumerable<PrefabConfiguration> configurations)
        {
            if (GameManager.Instance.GameMode == GameManager.EGameMode.Editor)
            {
                DestroyAllPaths();
            }

            yield return MapBuilder.StartCoroutine(BuildPrefabsCoroutine(configurations));
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
        /// <param name="isEditorBuild">Use True if new prefab is created for map layout. False is default for when map is being created as a whole.</param>
        /// <returns></returns>
        public bool BuildPrefab(PrefabConfiguration configuration, bool isEditorBuild = false)
        {
            if (configuration == null) return false;
            
            MapDescription.PrefabConfigurations ??= new List<PrefabConfiguration>();

            if (!MapDescription.PrefabConfigurations.Contains(configuration))
            {
                AddReplacePrefabConfiguration(configuration);
            }

            if (configuration.SpawnPrefabOnBuild)
            {
                GameObject newPrefab = BuildPhysicalPrefab(configuration);

                TriggerService.ProcessEmbeddedTriggerReceivers(newPrefab, isEditorBuild);
                _triggerService.ProcessAllEmbedded(newPrefab, isEditorBuild);
                _wallService.ProcessAllEmbedded(newPrefab, isEditorBuild);

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

            GameObject prefabGo = Prefabs.FirstOrDefault(go => go.GetComponent<PrefabBase>().Guid == configuration.Guid);

            if (!configuration.SpawnPrefabOnBuild) return;

            if (!prefabGo)
            {
                Logger.LogWarning($"No prefab of name \"{configuration.PrefabName}\" found for removal in Prefabs.");
                return;
            }

            Prefabs.Remove(prefabGo);
            
            _tilePrefabService.Remove(configuration);
            _wallService.Remove(configuration);
            _triggerService.Remove(configuration);
            _entryPointService.Remove(configuration);

            _triggerService.RemoveAllEmbedded(prefabGo);
            _wallService.RemoveAllEmbedded(prefabGo);

            prefabGo.transform.rotation = Quaternion.Euler(Vector3.zero);
            Transform offsetTransform = prefabGo.GetBody();
            if (offsetTransform) offsetTransform.localPosition = Vector3.zero;

            ObjectPool.Instance.Dismiss(prefabGo);
        }
        
        public void RemoveConfiguration(string guid) 
            => RemoveConfiguration(GetConfigurationByGuid<PrefabConfiguration>(guid));

        public void RemoveConfiguration(PrefabConfiguration configuration) 
            => MapDescription.PrefabConfigurations.Remove(configuration);

        public GameObject GetPrefabByGuid(string guid)
        {
            GameObject result = Prefabs.FirstOrDefault(p => p.GetComponent<PrefabBase>().Guid == guid);

            return !result ? null : result;
        }

        public GameObject GetPrefabByGridPosition(Vector3Int position)
        {
            GameObject result = Prefabs.FirstOrDefault(p => p.transform.position == position.ToWorldPositionV3Int());

            return !result ? null : result;
        }

        public IEnumerable<TP> GetPrefabsByPrefabType<TP>() where TP : PrefabBase =>
            Prefabs.Select(p => p.GetComponent<PrefabBase>()).OfType<TP>(); 

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

        public IEnumerable<TC> GetConfigurations<TC>(EPrefabType ePrefabType) where TC : PrefabConfiguration =>
            MapDescription.PrefabConfigurations.Where(c => c.PrefabType == ePrefabType)
                .Select(c => c as TC);

        public PrefabConfiguration GetPrefabConfigurationByTransformData(PositionRotation transformData)
        {
            return MapDescription.PrefabConfigurations.FirstOrDefault(c => c.TransformData.Equals(transformData));
        }

        public IEnumerable<TC> GetConfigurationsByPrefabClass<TC, TP>() where TP : PrefabBase where TC : PrefabConfiguration
        {
            List<TC> result = new();

            foreach (PrefabBase script in Prefabs.Select(go => go.GetComponent<PrefabBase>()))
            {
                if (script is TP @base) result.Add(GetConfigurationByGuid<TC>(@base.Guid));
            }

            return result;
        }

        public TC GetConfigurationByGuid<TC>(string guid) where TC : PrefabConfiguration =>
            MapDescription.PrefabConfigurations.Where(c => c.Guid == guid).FirstOrDefault() as TC;

        public bool GetConfigurationByOwnerGuidAndName<T>(string ownerGuid, string prefabName, out T configuration) where T : PrefabConfiguration
        {
            // prefab name is to distinguish more than one embedded prefab under same owner
            configuration = MapDescription.PrefabConfigurations
                .Where(c => c.OwnerGuid == ownerGuid && c.PrefabName == prefabName)
                .FirstOrDefault() as T;

            return configuration != null;
        }

        public void ChangePrefabPositionsBy(Vector3 positionChangeDelta)
        {
            foreach (PrefabConfiguration configuration in MapDescription.PrefabConfigurations)
            {
                configuration.TransformData.Position += positionChangeDelta;

                if (configuration is not WallConfiguration wall) continue;

                if (wall.WayPoints == null || !wall.WayPoints.Any()) continue;

                DestroyPath(EPathsType.Waypoint, wall.Guid);

                foreach (Waypoint waypoint in wall.WayPoints)
                {
                    waypoint.position += positionChangeDelta;
                }
            }
        }
        
        public IEnumerator ProcessPostBuildLayoutPrefabs()
        {
            TilePrefabService.ProcessPostBuild();
            
            yield return null;
        }

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

            prefabScript.Guid = configuration.Guid;

            _tilePrefabService.ProcessConfigurationOnBuild(configuration, prefabScript, newPrefab);
            _wallService.ProcessConfigurationOnBuild(configuration, prefabScript, newPrefab);
            _triggerService.ProcessConfigurationOnBuild(configuration, prefabScript, newPrefab);
            _entryPointService.ProcessConfigurationOnBuild(configuration, prefabScript, newPrefab);

            MapBuilder.Prefabs.Add(newPrefab);

            return newPrefab;
        }

        private int FindIndexOfConfiguration(PrefabConfiguration configuration) =>
            MapDescription.PrefabConfigurations.FindIndex(c => c.Guid == configuration.Guid);
    }
}