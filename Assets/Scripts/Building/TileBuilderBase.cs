using System.Collections.Generic;
using Scripts.Building.Tile;
using Scripts.Helpers;
using Scripts.System.Pooling;
using Unity.VisualScripting;
using UnityEngine;
using static Scripts.Building.Tile.TileDescription;
using Logger = Scripts.Helpers.Logger;

namespace Scripts.Building
{
    public abstract class TileBuilderBase
    {
        protected readonly Transform LayoutParent;
        protected TileController LastBuiltTile;
        protected readonly DefaultBuildPartsProvider DefaultsProvider;
        private readonly TileDescription[,] _layout;
        private readonly GameObject _tileDefaultPrefab;
        protected Dictionary<Vector3Int, GameObject> PhysicalTiles;

        protected TileBuilderBase(MapBuilder mapBuilder)
        {
            LayoutParent = mapBuilder.LayoutParent;
            DefaultsProvider = mapBuilder.defaultsProvider;
            PhysicalTiles = mapBuilder.PhysicalTiles;
            _layout = mapBuilder.Layout;
            _tileDefaultPrefab = mapBuilder.defaultsProvider.defaultTilePrefab;
        }

        public void BuildTile(int x, int y, TileDescription tileDescription = null)
        {
            tileDescription ??= _layout[x, y];
            if(tileDescription == null)
            {
                BuildNullTile(x, y);
            } 
            else
            {
                BuildNormalTile(x, y, tileDescription);
            }
        }

        protected virtual void BuildNullTile(int x, int y)
        {
        }

        protected virtual void BuildNormalTile(int x, int y, TileDescription tileDescription)
        {
            TileController newTile = ObjectPool.Instance.GetFromPool(_tileDefaultPrefab, LayoutParent.GameObject())
                .GetComponent<TileController>();
            
            newTile.gameObject.name = _tileDefaultPrefab.name;
            LastBuiltTile = newTile;
            Transform tileTransform = newTile.transform;

            foreach (ETileDirection direction in TileDescription.TileDirections)
            {
                WallDescription wall = tileDescription.GetWall(direction);

                if (wall == null)
                {
                    newTile.HideWall(direction);
                    continue;
                }

                newTile.ShowWall(direction);
                
                // Means default values
                if (wall.MeshInfo == null || !string.IsNullOrEmpty(wall.MeshInfo.materialName) && !string.IsNullOrEmpty(wall.MeshInfo.meshName)) continue;

                if (!string.IsNullOrEmpty(wall.MeshInfo.materialName) ^ !string.IsNullOrEmpty(wall.MeshInfo.meshName))
                {
                    Logger.LogError($"Tile at location: [{x}] [{y}] is missing either material or mesh for {direction}");
                }
            }
            
            tileTransform.position = new(x, 0f, y);
            PhysicalTiles.Add(tileTransform.position.ToVector3Int(), newTile.gameObject);
        }
    }
}