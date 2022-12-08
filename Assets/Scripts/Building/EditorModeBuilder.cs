﻿using Scripts.Building.Tile;
using Scripts.Helpers;
using Scripts.System.Pooling;
using UnityEngine;

namespace Scripts.Building
{
    public class EditorModeBuilder : TileBuilderBase
    {
        private readonly Vector3 _tileScaleInEditor = new(0.99f, 0.99f, 0.99f);
        
        public EditorModeBuilder(MapBuilder mapBuilder) : base(mapBuilder)
        {
        }

        protected override void BuildNullTile(int floor, int row,int column)
        {
            KeyVector.x = row;
            KeyVector.y = floor;
            KeyVector.z = column;

            // Try to find what block is in PhysicalTiles on that location
            if (PhysicalTiles.TryGetValue(KeyVector, out GameObject foundTile))
            {
                // There is already null tile, so stop an execution
                if (!foundTile.GetComponent<TileController>())
                {
                    return;
                }
                // There is physical tile already, let us dispose of it.
                ObjectPool.Instance.ReturnToPool(foundTile);
                PhysicalTiles.Remove(KeyVector);
            }
            
            GameObject cube = DefaultsProvider.defaultNullCubePrefab
                ? ObjectPool.Instance.GetFromPool(DefaultsProvider.defaultNullCubePrefab, LayoutParent.gameObject)
                : GameObject.CreatePrimitive(PrimitiveType.Cube);
            
            cube.transform.parent = LayoutParent;
            cube.transform.position = new Vector3(row, 0 - floor, column);
            cube.transform.localScale = _tileScaleInEditor;
            
            if (floor != MapBuilder.MapDescription.StartGridPosition.x)
            {
                cube.SetActive(false);
            }

            PhysicalTiles.Add(cube.transform.position.ToVector3Int(), cube);
        }

        protected override void BuildNormalTile(int floor, int row, int column, TileDescription tileDescription)
        {
            base.BuildNormalTile(floor, row, column, tileDescription);

            LastBuiltTile.HideWall(TileDescription.ETileDirection.Ceiling);
            LastBuiltTile.transform.localScale = _tileScaleInEditor;

            if (floor != MapBuilder.MapDescription.StartGridPosition.x)
            {
                LastBuiltTile.gameObject.SetActive(false);
            }
        }
    }
}