using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Scripts.Building.PrefabsSpawning.Configurations;
using Scripts.Building.PrefabsSpawning.Walls;
using Scripts.Building.Tile;
using Scripts.Helpers.Extensions;
using Scripts.MapEditor;
using Scripts.MapEditor.Services;
using Scripts.ScriptableObjects;
using Scripts.System;
using Scripts.System.Pooling;
using Unity.VisualScripting;
using UnityEngine;
using Logger = Scripts.Helpers.Logger;

namespace Scripts.Building
{
    public class PrefabBuilder
    {
        private static MapBuilder MapBuilder => GameManager.Instance.MapBuilder;
        private static MapDescription MapDescription => MapBuilder.MapDescription;
        private static HashSet<GameObject> Prefabs => MapBuilder.Prefabs;
        private static TileDescription[,,] Layout => MapBuilder.Layout;

        internal void BuildPrefabs(IEnumerable<PrefabConfiguration> configurations) =>
            MapBuilder.StartCoroutine(BuildPrefabsCoroutine(configurations));

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
            if (!MapDescription.PrefabConfigurations.Contains(configuration))
            {
                AddReplacePrefabConfiguration(configuration);
            }
            
            bool isEditorMode = GameManager.Instance.GameMode is GameManager.EGameMode.Editor;

            GameObject newPrefab = PrefabStore.Instantiate(configuration.PrefabName, MapBuilder.PrefabsParent);
            

            if (!newPrefab)
            {
                Logger.LogError($"Prefab \"{configuration.PrefabName}\" was not found.");
                MapBuilder.MapDescription.PrefabConfigurations.Remove(configuration);
                return false;
            }

            newPrefab.transform.position = configuration.TransformData.Position;

            if (configuration is TilePrefabConfiguration)
            {
                newPrefab.GetBody().rotation = configuration.TransformData.Rotation;
            }

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

                if (isEditorMode)
                {
                    WallPrefabBase script = newPrefab.GetComponent<WallPrefabBase>();

                    if (script && script.presentedInEditor)
                    {
                        script.presentedInEditor.SetActive(true);
                    }

                    if (wallConfiguration.HasPath())
                    {
                        WayPointService.AddPath(wallConfiguration.WayPoints);
                    }
                }
            }

            MapDescription.PrefabConfigurations ??= new List<PrefabConfiguration>();

            MapBuilder.Prefabs.Add(newPrefab);

            if (isEditorMode && -newPrefab.transform.position.y < MapEditorManager.Instance.CurrentFloor)
            {
                newPrefab.SetActive(false);
            }

            return true;
        }

        public void RemovePrefab(PrefabConfiguration configuration)
        {
            if (configuration == null) return;

            PrefabConfiguration config = MapDescription.PrefabConfigurations.FirstOrDefault(c =>
                c.PrefabName == configuration.PrefabName
                && c.TransformData == configuration.TransformData);

            if (config == null) return;

            MapDescription.PrefabConfigurations.Remove(config);

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
                WayPointService.DestroyPath(wall.WayPoints);
            }

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

        public IEnumerable<T> GetPrefabConfigurationsOnWorldPosition<T>(Vector3 worldPosition) where T : PrefabConfiguration
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
                Logger.Log("replacing configuration");
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

                WayPointService.DestroyPath(wall.WayPoints);

                foreach (Waypoint waypoint in wall.WayPoints)
                {
                    waypoint.position += positionChangeDelta;
                }
            }
        }

        public PrefabConfiguration GetPrefabConfigurationByTransformData(PositionRotation transformData) =>
            MapDescription.PrefabConfigurations.FirstOrDefault(c => c.TransformData == transformData);

        private int FindIndexOfConfiguration(PrefabConfiguration configuration) =>
            MapDescription.PrefabConfigurations.FindIndex(c => c.TransformData == configuration.TransformData);
    }
}