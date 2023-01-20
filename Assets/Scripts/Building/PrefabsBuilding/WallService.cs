using Scripts.Building.PrefabsSpawning.Configurations;
using Scripts.Building.PrefabsSpawning.Walls;
using Scripts.Building.Tile;
using Scripts.Building.Walls;
using Scripts.Helpers.Extensions;
using Scripts.System;
using UnityEngine;
using static Scripts.MapEditor.Services.PathsService;

namespace Scripts.Building.PrefabsBuilding
{
    public static class WallService
    {
        private static MapBuilder MapBuilder => GameManager.Instance.MapBuilder;
        private static bool IsInEditMode => GameManager.Instance.GameMode is GameManager.EGameMode.Editor;

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

        public static void SetForMovement(Vector3 worldPosition, bool isWalkable) 
            => MapBuilder.Layout.ByGridV3Int(worldPosition.ToGridPosition()).IsForMovement = isWalkable;
    }
}