using System.Collections.Generic;
using Scripts.Building.PrefabsSpawning;
using Scripts.Building.PrefabsSpawning.Configurations;
using Scripts.Building.Tile;
using Scripts.EventsManagement;
using Scripts.Helpers.Extensions;
using Scripts.System;
using UnityEngine;
using static Scripts.Building.Tile.TileDescription;

namespace Scripts.Building.PrefabsBuilding
{
    public static class TilePrefabService
    {
        private static MapBuilder MapBuilder => GameManager.Instance.MapBuilder;
        private static bool IsInEditMode => GameManager.Instance.GameMode == GameManager.EGameMode.Editor;

        public static readonly Dictionary<string, TilePrefab> TilePrefabs;

        static TilePrefabService()
        {
            TilePrefabs = new Dictionary<string, TilePrefab>();
            EventsManager.OnMapDemolished += () => TilePrefabs.Clear();
        }

        public static void ProcessConfigurationOnBuild(PrefabConfiguration configuration, PrefabBase prefabScript, GameObject newPrefab)
        {
            if (configuration is not TilePrefabConfiguration tileConfiguration|| prefabScript is not TilePrefab script) return;
            
            newPrefab.GetBody().rotation = configuration.TransformData.Rotation;

            TilePrefabs.TryAdd(configuration.Guid, script);

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

            if (TilePrefabs.ContainsKey(configuration.Guid)) TilePrefabs.Remove(configuration.Guid);
            
            MapBuilder.SetTileForMovement(prefabScript.transform.position, true);

            if (IsInEditMode)
            {
                if (prefabScript)
                {
                    TileController prefabTile = MapBuilder.GetPhysicalTileByWorldPosition(prefabScript.transform.position)
                        .GetComponent<TileController>();
                    if (script.disableFloor) prefabTile.ShowWall(ETileDirection.Floor);
                    if (script.disableCeiling) prefabTile.ShowWall(ETileDirection.Ceiling);
                }
            }
        }

        public static void ProcessPostBuild()
        {
            foreach (TilePrefab tilePrefab in TilePrefabs.Values)
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