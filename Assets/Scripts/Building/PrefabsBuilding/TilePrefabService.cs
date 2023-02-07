using Scripts.Building.PrefabsSpawning;
using Scripts.Building.PrefabsSpawning.Configurations;
using Scripts.Building.Tile;
using UnityEngine;
using static Scripts.Building.Tile.TileDescription;
using Logger = Scripts.Helpers.Logger;

namespace Scripts.Building.PrefabsBuilding
{
    public class TilePrefabService : PrefabServiceBase<TilePrefabConfiguration, TilePrefab>
    {
        protected override TilePrefabConfiguration GetConfigurationFromPrefab(PrefabBase prefab, string ownerGuid, bool spawnPrefabOnBuild) 
            => new(prefab as TilePrefab, ownerGuid, spawnPrefabOnBuild);

        public override void ProcessEmbedded(GameObject newPrefab)
        {
            Logger.LogNotImplemented();
        }

        protected override void RemoveEmbedded(TilePrefab prefabGo)
        {
            Logger.LogNotImplemented();
        }

        protected override void ProcessConfiguration(TilePrefabConfiguration configuration, TilePrefab script, GameObject newPrefab)
        {
            // IsWalkable is set in Layout in post-build to prevent race condition with building tiles, so here only on prefab
            script.isWalkable = configuration.IsWalkable;
            
            // To update map when spawning new tile prefab.
            // if (!IsInEditMode) return;
            
            if (script.disableFloor) WallService.ActivateWall(script.transform.position, ETileDirection.Floor, false);
            if (script.disableCeiling) WallService.ActivateWall(script.transform.position, ETileDirection.Ceiling, false);
        }
        
        protected override void RemoveConfiguration(TilePrefabConfiguration configuration)
        {
            Vector3 worldPosition = GetGameObject(configuration.Guid).transform.position;
            
            MapBuilder.SetTileForMovement(worldPosition, true);

            if (!IsInEditMode) return;
            
            TilePrefab script = GetPrefabScript(configuration.Guid);
            TileController prefabTile = MapBuilder.GetPhysicalTileByWorldPosition(worldPosition).GetComponent<TileController>();
            if (script.disableFloor) prefabTile.ShowWall(ETileDirection.Floor);
            if (script.disableCeiling) prefabTile.ShowWall(ETileDirection.Ceiling);
        }

        public static void ProcessPostBuild()
        {
            foreach (TilePrefab tilePrefab in PrefabScripts)
            {
                Vector3 worldPosition = tilePrefab.transform.position;
                
                if (tilePrefab.disableFloor)
                {
                    WallService.ActivateWall(worldPosition, ETileDirection.Floor, false);
                }
                
                if (tilePrefab.disableCeiling)
                {
                    WallService.ActivateWall(worldPosition, ETileDirection.Ceiling, false);
                }
                
                MapBuilder.SetTileForMovement(worldPosition, tilePrefab.isWalkable);
            }
        }
    }
}