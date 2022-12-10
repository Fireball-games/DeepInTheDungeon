using Scripts.Building.Tile;
using Scripts.Helpers;
using Scripts.MapEditor;
using Scripts.System;
using Scripts.System.Pooling;
using UnityEngine;
using static Scripts.MapEditor.Enums;

namespace Scripts.Building
{
    public class EditorModeBuilder : TileBuilderBase
    {
        private readonly Vector3 _tileScaleInEditor = new(0.95f, 0.95f, 0.95f);
        
        public EditorModeBuilder(MapBuilder mapBuilder) : base(mapBuilder)
        {
        }

        protected override void BuildNullTile(int floor, int row,int column, bool isRebuilding = false)
        {
            WorldKey.x = row;
            WorldKey.y = -floor;
            WorldKey.z = column;

            // Try to find what block is in PhysicalTiles on that location
            if (PhysicalTiles.TryGetValue(WorldKey, out GameObject foundTile))
            {
                // There is already null tile, so stop an execution
                if (!foundTile.GetComponent<TileController>())
                {
                    return;
                }
                // There is physical tile already, let us dispose of it.
                ObjectPool.Instance.ReturnToPool(foundTile);
                PhysicalTiles.Remove(WorldKey);
            }
            
            GameObject cube = DefaultsProvider.defaultNullCubePrefab
                ? ObjectPool.Instance.GetFromPool(DefaultsProvider.defaultNullCubePrefab, LayoutParent.gameObject)
                : GameObject.CreatePrimitive(PrimitiveType.Cube);
            
            cube.transform.parent = LayoutParent;
            cube.transform.position = new Vector3(row, 0 - floor, column);
            cube.transform.localScale = _tileScaleInEditor;

            SetDisabledIfApplicable(cube, floor, isRebuilding);

            PhysicalTiles.Add(cube.transform.position.ToVector3Int(), cube);
        }

        protected override void BuildNormalTile(int floor, int row, int column, TileDescription tileDescription, bool isRebuilding = false)
        {
            base.BuildNormalTile(floor, row, column, tileDescription, isRebuilding);

            LastBuiltTile.HideWall(TileDescription.ETileDirection.Ceiling);
            LastBuiltTile.transform.localScale = _tileScaleInEditor;

            SetDisabledIfApplicable(LastBuiltTile.gameObject, floor, isRebuilding);
        }

        private void SetDisabledIfApplicable(GameObject tile, int floor, bool isRebuilding = false)
        {
            // Means on or bellow start level TODO: replace with list of displayed floors when implemented floor show status
            if (!isRebuilding && floor < MapBuilder.MapDescription.StartGridPosition.x)
            {
                tile.SetActive(false);
            }
        }
    }
}