using Scripts.Building.Tile;
using UnityEngine;

namespace Scripts.Building
{
    public class EditorModeBuilder : TileBuilderBase
    {
        private Vector3 _tileScaleInEditor = new(0.99f, 0.99f, 0.99f);
        
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
        }

        protected override void BuildNormalTile(int x, int y)
        {
            base.BuildNormalTile(x, y);

            LastBuiltTile.HideWall(TileDescription.ETileDirection.Ceiling);
            LastBuiltTile.transform.localScale = _tileScaleInEditor;
        }
    }
}