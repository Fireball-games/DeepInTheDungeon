using Scripts.Building.Tile;
using UnityEngine;
using static Scripts.Building.Tile.TileDescription;
using Logger = Scripts.Helpers.Logger;

namespace Scripts.Building
{
    public abstract class TileBuilderBase
    {
        protected readonly Transform LayoutParent;
        protected TileController LastBuiltTile;
        protected DefaultBuildPartsProvider DefaultsProvider;
        private readonly TileDescription[,] _layout;
        private readonly GameObject _tileDefaultPrefab;
        
        protected TileBuilderBase(MapBuilder mapBuilder)
        {
            LayoutParent = mapBuilder.LayoutParent;
            DefaultsProvider = mapBuilder.defaultsProvider;
            _layout = mapBuilder.Layout;
            _tileDefaultPrefab = mapBuilder.DefaultTile;
        }

        public void BuildTile(int x, int y)
        {
            if(_layout[x, y] == null)
            {
                BuildNullTile(x, y);
            } 
            else
            {
                BuildNormalTile(x, y);
            }
        }

        protected virtual void BuildNullTile(int x, int y)
        {
        }

        protected virtual void BuildNormalTile(int x, int y)
        {
            TileController newTile = GameObject.Instantiate(_tileDefaultPrefab, LayoutParent).GetComponent<TileController>();
            LastBuiltTile = newTile;
            Transform tileTransform = newTile.transform;

            foreach (ETileDirection direction in TileDirections)
            {
                WallDescription wall = _layout[x, y].GetWall(direction);

                if (wall == null)
                {
                    newTile.HideWall(direction);
                    continue;
                }
                
                // Means default values
                if (wall.RenderingInfo == null || !wall.RenderingInfo.material && !wall.RenderingInfo.mesh) continue;

                if (!wall.RenderingInfo.material ^ !wall.RenderingInfo.mesh)
                {
                    Logger.LogError($"Tile at location: [{x}] [{y}] is missing either material or mesh for {direction}");
                }
            }
            
            tileTransform.position = new(x, 0f, y);
        }
    }
}