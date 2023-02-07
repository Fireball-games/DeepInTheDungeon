using Scripts.Building.PrefabsSpawning;
using Scripts.Building.PrefabsSpawning.Configurations;
using Scripts.Building.PrefabsSpawning.Walls;
using Scripts.Building.Tile;
using Scripts.Helpers.Extensions;
using UnityEngine;
using static Scripts.MapEditor.Services.PathsService;
using Logger = Scripts.Helpers.Logger;

namespace Scripts.Building.PrefabsBuilding
{
    public class WallService : PrefabServiceBase<WallConfiguration, WallPrefabBase>
    {
        public static void ActivateWall(Vector3 tileWorldPosition, TileDescription.ETileDirection wallDirection, bool isActive)
        {
            TileController tile = MapBuilder.GetPhysicalTileByWorldPosition(tileWorldPosition).GetComponent<TileController>();

            if (isActive)
                tile.ShowWall(wallDirection);
            else
                tile.HideWall(wallDirection);
        }

        public static void Remove(PrefabConfiguration configuration)
        {
            if (configuration is WallConfiguration wall)
            {
                RemoveFromStore(configuration.Guid);
                
                if (wall.HasPath())
                {
                    DestroyPath(EPathsType.Waypoint, wall.Guid);
                }
            }
        }

        public static void ProcessConfigurationOnBuild(PrefabConfiguration configuration, PrefabBase prefabScript, GameObject newPrefab)
        {
            if (configuration is not WallConfiguration wallConfiguration) return;
            
            newPrefab.transform.localRotation = configuration.TransformData.Rotation;

            Transform physicalPart = newPrefab.GetBody();

            if (physicalPart)
            {
                Vector3 position = physicalPart.localPosition;
                position.x += wallConfiguration.Offset;
                physicalPart.localPosition = position;
            }

            if (!IsInEditMode || prefabScript is not WallPrefabBase script) return;
            
            AddToStore(wallConfiguration, script, newPrefab);
            
            if (script && script.presentedInEditor)
            {
                script.presentedInEditor.SetActive(true);
            }

            if (wallConfiguration.HasPath())
            {
                AddReplaceWaypointPath(wallConfiguration.Guid);
            }
        }

        public override void ProcessEmbeddedPrefabs(GameObject newPrefab)
        {
            PrefabBase prefabScript = newPrefab.GetComponent<PrefabBase>();

            if (!prefabScript) return;

            foreach (WallPrefabBase embeddedWall in newPrefab.GetComponentsInChildren<WallPrefabBase>())
            {
                WallConfiguration configuration;
                // We dont want to process only embedded walls, not owners
                if (prefabScript.Guid == embeddedWall.Guid) continue;

                if (IsInEditMode)
                {
                    configuration = AddConfigurationToMap(embeddedWall, prefabScript.Guid);
                    AddToStore(configuration, embeddedWall, embeddedWall.gameObject);
                    if (configuration.HasPath()) AddReplaceWaypointPath(configuration.Guid);
                }
                else
                {
                    if (!MapBuilder.GetConfigurationByOwnerGuidAndName(prefabScript.Guid, embeddedWall.gameObject.name,
                            out configuration))
                    {
                        Logger.LogWarning("Failed to find configuration for wall", logObject: embeddedWall);
                        continue;
                    }
                }

                if (embeddedWall.presentedInEditor) embeddedWall.presentedInEditor.SetActive(IsInEditMode);
                embeddedWall.Guid = configuration.Guid;
            }
        }

        public override void RemoveEmbeddedPrefabs(GameObject prefabGo)
        {
            PrefabBase prefabScript = prefabGo.GetComponent<PrefabBase>();

            if (!prefabScript) return;

            foreach (WallPrefabBase wall in prefabGo.GetComponentsInChildren<WallPrefabBase>())
            {
                if (IsInEditMode && wall is WallMovement wallMovement)
                {
                    DestroyPath(EPathsType.Waypoint, wallMovement.Guid);
                }
                
                RemoveFromStore(wall.Guid);
                MapBuilder.RemoveConfiguration(wall.Guid);
            }
        }

        protected override WallConfiguration GetConfigurationFromPrefab(PrefabBase prefab, string ownerGuid, bool spawnPrefabOnBuild)
        {
            return prefab is not WallPrefabBase wallBase 
                ? null 
                : new WallConfiguration(wallBase, ownerGuid, spawnPrefabOnBuild);
        }
    }
}