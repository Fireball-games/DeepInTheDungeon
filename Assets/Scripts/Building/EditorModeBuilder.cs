﻿
using Scripts.Helpers;

namespace Scripts.Building
{
    public class EditorModeBuilder : TileBuilderBase
    {
        public EditorModeBuilder(MapBuilder mapBuilder)
        {
            LayoutParent = mapBuilder.LayoutParent;
            Layout = mapBuilder.Layout;
            FloorPrefab = mapBuilder.FloorPrefab;
            CeilingPrefab = mapBuilder.CeilingPrefab;
            WallPrefab = mapBuilder.WallPrefab;
        }
        
        public override void BuildTile(int x, int y)
        {
            Logger.LogWarning("NOT IMPLEMENTED YET");
        }

        protected override void BuildBaseTile(int x, int y)
        {
            Logger.LogWarning("NOT IMPLEMENTED YET");
        }
    }
}