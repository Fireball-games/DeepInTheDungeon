using System.Collections.Generic;
using Scripts.Building.Tile;
using UnityEngine;

namespace Scripts.Building
{
    public abstract class TileBuilderBase
    {
        protected Transform LayoutParent;
        protected List<List<TileDescription>> Layout;
        protected GameObject FloorPrefab;
        protected GameObject CeilingPrefab;
        protected GameObject WallPrefab;

        public abstract void BuildTile(int x, int y);

        protected abstract void BuildBaseTile(int x, int y);
    }
}