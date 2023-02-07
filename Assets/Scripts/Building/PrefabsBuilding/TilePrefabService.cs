using Scripts.Building.PrefabsSpawning;
using Scripts.Building.PrefabsSpawning.Configurations;
using Scripts.Building.Tile;
using Scripts.Helpers.Extensions;
using UnityEngine;
using static Scripts.Building.Tile.TileDescription;
using Logger = Scripts.Helpers.Logger;

namespace Scripts.Building.PrefabsBuilding
{
    public class TilePrefabService : PrefabServiceBase<TilePrefabConfiguration, TilePrefab>
    {
        protected override TilePrefabConfiguration GetConfigurationFromPrefab(PrefabBase prefab, string ownerGuid, bool spawnPrefabOnBuild) 
            => new(prefab as TilePrefab, ownerGuid, spawnPrefabOnBuild);

        public override void ProcessEmbeddedPrefabs(GameObject newPrefab)
        {
            Logger.LogNotImplemented();
        }

        public override void RemoveEmbeddedPrefabs(GameObject prefabGo)
        {
            Logger.LogNotImplemented();
        }

        public static void ProcessConfigurationOnBuild(PrefabConfiguration configuration, PrefabBase prefabScript, GameObject newPrefab)
        {
            if (configuration is not TilePrefabConfiguration tileConfiguration|| prefabScript is not TilePrefab script) return;
            
            newPrefab.GetBody().rotation = configuration.TransformData.Rotation;

            AddToStore(tileConfiguration, script, newPrefab);

            // IsWalkable is set in Layout in post-build to prevent race condition with building tiles, so here only on prefab
            script.isWalkable = tileConfiguration.IsWalkable;
            // To update map when spawning new tile prefab.
            if (IsInEditMode)
            {
                if (script.disableFloor) WallService.ActivateWall(prefabScript.transform.position, ETileDirection.Floor, false);
                if (script.disableCeiling) WallService.ActivateWall(prefabScript.transform.position, ETileDirection.Ceiling, false);
            }
            
        }
        
        public static void Remove(PrefabConfiguration configuration, PrefabBase prefabScript)
        {
            if (configuration is not TilePrefabConfiguration || prefabScript is not TilePrefab script) return;
            
            MapBuilder.SetTileForMovement(prefabScript.transform.position, true);

            if (!IsInEditMode) return;
            
            RemoveFromStore(configuration.Guid);

            if (!prefabScript) return;
            
            TileController prefabTile = MapBuilder.GetPhysicalTileByWorldPosition(prefabScript.transform.position)
                .GetComponent<TileController>();
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