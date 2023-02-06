using Scripts.Building.PrefabsSpawning.Configurations;
using Scripts.Building.PrefabsSpawning.Walls;
using Scripts.Building.Tile;
using Scripts.Building.Walls;
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
            if (configuration is WallConfiguration wall && wall.HasPath())
            {
                DestroyPath(EPathsType.Waypoint, wall.Guid);
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
                
            if (script && script.presentedInEditor)
            {
                script.presentedInEditor.SetActive(true);
            }

            if (wallConfiguration.HasPath())
            {
                AddReplaceWaypointPath(wallConfiguration.Guid, wallConfiguration.WayPoints);
            }
        }

        public void ProcessEmbeddedWalls(GameObject newPrefab)
        {
            PrefabBase prefabScript = newPrefab.GetComponent<PrefabBase>();

            if (!prefabScript) return;

            foreach (WallPrefabBase embeddedWall in newPrefab.GetComponents<WallPrefabBase>())
            {
                WallConfiguration configuration;

                if (IsInEditMode)
                {
                    configuration = AddConfigurationToMap(embeddedWall, prefabScript.Guid);
                    AddToStore(configuration, embeddedWall, newPrefab);
                }
                else
                {
                    if (!MapBuilder.GetConfigurationByOwnerGuidAndName(prefabScript.Guid, prefabScript.gameObject.name,
                            out configuration))
                    {
                        Logger.LogWarning("Failed to find configuration for trigger", logObject: embeddedWall);
                        continue;
                    }
                }

                embeddedWall.Guid = configuration.Guid;
            }
        }

        public static void RemoveEmbeddedWalls(GameObject prefabGo)
        {
            Logger.LogNotImplemented();
        }

        protected override WallConfiguration GetConfigurationFromPrefab(PrefabBase prefab, string ownerGuid, bool spawnPrefabOnBuild)
        {
            return prefab is not WallPrefabBase wallBase 
                ? null 
                : new WallConfiguration(wallBase, ownerGuid, spawnPrefabOnBuild);
        }
    }
}