using Scripts.Building.Tile;
using UnityEngine;

namespace Scripts.Building
{
    public class EditorModeBuilder : TileBuilderBase
    {
        private readonly Vector3 _tileScaleInEditor = new(0.99f, 0.99f, 0.99f);
        
        public EditorModeBuilder(MapBuilder mapBuilder) : base(mapBuilder)
        {
        }

        protected override void BuildNullTile(int x, int y)
        {
            GameObject cube = DefaultsProvider.defaultNullCubePrefab
                ? GameObject.Instantiate(DefaultsProvider.defaultNullCubePrefab, LayoutParent)
                : GameObject.CreatePrimitive(PrimitiveType.Cube);
            
            cube.transform.parent = LayoutParent;
            cube.transform.position = new Vector3(x, 0f, y);
            cube.transform.localScale = _tileScaleInEditor;
            cube.name = $"Tile: x: {x}, y: {y}";
        }

        protected override void BuildNormalTile(int x, int y, TileDescription tileDescription)
        {
            base.BuildNormalTile(x, y, tileDescription);

            LastBuiltTile.HideWall(TileDescription.ETileDirection.Ceiling);
            LastBuiltTile.transform.localScale = _tileScaleInEditor;
        }
    }
}