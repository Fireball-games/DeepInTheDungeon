using System.Collections.Generic;
using Scripts.Building.Tile;
using UnityEngine;

namespace Scripts.Building
{
    public abstract class TileBuilderBase
    {
        protected Transform LayoutParent;
        protected TileDescription[,] Layout;
        protected GameObject TileDefaultPrefab;
        
        protected TileBuilderBase(MapBuilder mapBuilder)
        {
            LayoutParent = mapBuilder.LayoutParent;
            Layout = mapBuilder.Layout;
            TileDefaultPrefab = mapBuilder.DefaultTile;
        }

        public abstract void BuildTile(int x, int y);

        protected abstract void BuildBaseTile(int x, int y);
    }
}