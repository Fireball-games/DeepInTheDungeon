using Scripts.Building.Tile;
using UnityEngine;

namespace Scripts.Building
{
    public class EditorModeBuilder : TileBuilderBase
    {
        public EditorModeBuilder(MapBuilder mapBuilder) : base(mapBuilder)
        {}

        protected override void BuildNullTile(int x, int y)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = new Vector3(x, 0f, y);
        }

        protected override void BuildNormalTile(int x, int y)
        {
            base.BuildNormalTile(x, y);
            
            LastBuiltTile.HideWall(TileDescription.ETileDirection.Ceiling);
        }
    }
}