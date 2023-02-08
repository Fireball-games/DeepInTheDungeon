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
        public static void ActivateWall(Vector3 tileWorldPosition, TileDescription.ETileDirection wallDirection,
            bool isActive)
        {
            TileController tile = MapBuilder.GetPhysicalTileByWorldPosition(tileWorldPosition)
                .GetComponent<TileController>();

            if (isActive)
                tile.ShowWall(wallDirection);
            else
                tile.HideWall(wallDirection);
        }

        protected override void RemoveConfiguration(WallConfiguration configuration)
        {
            if (configuration.HasPath())
            {
                DestroyPath(EPathsType.Waypoint, configuration.Guid);
            }
        }

        protected override void ProcessConfiguration(WallConfiguration configuration, WallPrefabBase prefabScript,
            GameObject newPrefab)
        {
            Transform physicalPart = newPrefab.GetBody();

            if (physicalPart)
            {
                Vector3 position = physicalPart.localPosition;
                position.x += configuration.Offset;
                physicalPart.localPosition = position;
            }

            if (!IsInEditMode) return;

            if (prefabScript && prefabScript.presentedInEditor)
            {
                prefabScript.presentedInEditor.SetActive(true);
            }

            if (configuration.HasPath())
            {
                AddReplaceWaypointPath(configuration.Guid);
            }
        }
        
        protected override void ProcessEmbedded(WallConfiguration configuration, WallPrefabBase prefabScript)
        {
            if (IsInEditMode && configuration.HasPath())
            {
                AddReplaceWaypointPath(configuration.Guid);
            }
        }

        protected override void RemoveEmbedded(WallPrefabBase embeddedWall)
        {
            if (IsInEditMode && embeddedWall is WallMovement wallMovement)
            {
                DestroyPath(EPathsType.Waypoint, wallMovement.Guid);
            }
        }

        protected override WallConfiguration GetConfigurationFromPrefab(PrefabBase prefab, string ownerGuid,
            bool spawnPrefabOnBuild)
        {
            return prefab is not WallPrefabBase wallBase
                ? null
                : new WallConfiguration(wallBase, ownerGuid, spawnPrefabOnBuild);
        }
    }
}