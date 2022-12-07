using System;
using System.Collections.Generic;
using Scripts.Building.Tile;
using Scripts.Helpers;
using Scripts.System.Pooling;
using UnityEngine;
using Logger = Scripts.Helpers.Logger;

namespace Scripts.Building
{
    public abstract class TileBuilderBase
    {
        protected readonly Transform LayoutParent;
        protected readonly DefaultBuildPartsProvider DefaultsProvider;
        private readonly GameObject _tileDefaultPrefab;
        protected Dictionary<Vector3Int, GameObject> PhysicalTiles => _mapBuilder.PhysicalTiles;
        protected TileController LastBuiltTile;
        protected Vector3Int KeyVector;
        private TileDescription[,] Layout => _mapBuilder.Layout;
        private readonly MapBuilder _mapBuilder;

        protected TileBuilderBase(MapBuilder mapBuilder)
        {
            _mapBuilder = mapBuilder;
            LayoutParent = mapBuilder.LayoutParent;
            DefaultsProvider = mapBuilder.defaultsProvider;
            _tileDefaultPrefab = mapBuilder.defaultsProvider.defaultTilePrefab;
        }

        public void BuildTile(int x, int y, TileDescription tileDescription = null)
        {
            tileDescription ??= Layout[x, y];
            if(tileDescription == null)
            {
                BuildNullTile(x, y);
            } 
            else
            {
                BuildNormalTile(x, y, tileDescription);
            }
        }

        protected virtual void BuildNullTile(int row, int column)
        {
        }
        
        protected virtual void BuildNormalTile(int row, int column, TileDescription tileDescription)
        {
            KeyVector.x = row;
            KeyVector.z = column;

            tileDescription ??= Layout[row, column];

            TileController newTile = null;
            // Try to find what block is in PhysicalTiles on that location
            if (PhysicalTiles.TryGetValue(KeyVector, out GameObject foundTile))
            {
                newTile = foundTile.GetComponent<TileController>();
                // If its not a tile (could be null tile), then get rid of it
                if (!newTile)
                {
                    ObjectPool.Instance.ReturnToPool(foundTile);
                    PhysicalTiles.Remove(KeyVector);
                }
            }
            
            // Means there was no found file or if so, it wasn't a tile, so it got disposed, so we need a new tile. We know, that in layout is
            // not a null tile, because else it would not be here.
            if (!newTile)
            {
                newTile = ObjectPool.Instance.GetFromPool(_tileDefaultPrefab, LayoutParent.gameObject).GetComponent<TileController>();
            }

            newTile.gameObject.name = _tileDefaultPrefab.name;
            LastBuiltTile = newTile;
            Transform tileTransform = newTile.transform;

            foreach (Vector3Int direction in TileDirections.VectorDirections)
            {
                if (Layout[row+direction.x, column+direction.z] == null )
                {
                    newTile.ShowWall(TileDirections.WallDirectionByVector[direction]);
                }
                else
                {
                    newTile.HideWall(TileDirections.WallDirectionByVector[direction]);
                }
                
                WallDescription wall = tileDescription.GetWall(direction);
                
                // Means default values
                if (wall.MeshInfo == null || !string.IsNullOrEmpty(wall.MeshInfo.materialName) && !string.IsNullOrEmpty(wall.MeshInfo.meshName)) continue;
                // Means only material OR mesh name is set, which is an error
                if (!string.IsNullOrEmpty(wall.MeshInfo.materialName) ^ !string.IsNullOrEmpty(wall.MeshInfo.meshName))
                {
                    Logger.LogError($"Tile at location: [{row}] [{column}] is missing either material or mesh for {direction}");
                }
                
                // TODO: manage mesh and material via stored names
            }
            
            tileTransform.position = new(row, 0f, column);
            
            // TODO: this is caused with MapEditorManager runs Start twice due to some mysterious intervention, could not solve it myself at the time
            try
            {
                PhysicalTiles.Add(tileTransform.position.ToVector3Int(), newTile.gameObject);
            }
            catch (Exception)
            {
                 // Logger.LogWarning("Trying to add Physical tile where it should not be possible");
            }
        }
    }
}