using System.Collections.Generic;
using Scripts.Building.PrefabsSpawning.Configurations;
using Scripts.Building.Tile;
using Scripts.Building.Walls;
using Scripts.EventsManagement;
using Scripts.Helpers.Extensions;
using Scripts.System;
using UnityEngine;

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
            if (configuration is not TilePrefabConfiguration || prefabScript is not TilePrefab script) return;
            
            newPrefab.GetBody().rotation = configuration.TransformData.Rotation;

            TilePrefabs.TryAdd(configuration.Guid, script);
        }
        
        public static void Remove(PrefabConfiguration configuration, PrefabBase prefabScript)
        {
            if (configuration is not TilePrefabConfiguration || prefabScript is not TilePrefab script) return;

            if (TilePrefabs.ContainsKey(configuration.Guid)) TilePrefabs.Remove(configuration.Guid);
            
            WallService.SetForMovement(prefabScript.transform.position, true);

            if (IsInEditMode)
            {
                if (prefabScript)
                {
                    TileController prefabTile = MapBuilder.GetPhysicalTileByWorldPosition(prefabScript.transform.position)
                        .GetComponent<TileController>();
                    if (script.disableFloor) prefabTile.ShowWall(TileDescription.ETileDirection.Floor);
                    if (script.disableCeiling) prefabTile.ShowWall(TileDescription.ETileDirection.Ceiling);
                }
            }
        }

        public static void ProcessPostBuild()
        {
            foreach (TilePrefab tilePrefab in TilePrefabs.Values) 
            {
                if (tilePrefab.disableFloor)
                {
                    MapBuilder.GetPhysicalTileByWorldPosition(tilePrefab.transform.position)
                        .GetComponent<TileController>()
                        .HideWall(TileDescription.ETileDirection.Floor);
                }
                
                if (tilePrefab.disableCeiling)
                {
                    MapBuilder.GetPhysicalTileByWorldPosition(tilePrefab.transform.position)
                        .GetComponent<TileController>()
                        .HideWall(TileDescription.ETileDirection.Ceiling);
                }
            }
        }
    }
}