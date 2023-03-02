﻿using System.Collections.Generic;
using Scripts.Building.Tile;
using Scripts.Helpers.Extensions;
using Scripts.System.Pooling;
using UnityEngine;

namespace Scripts.Building
{
    public class EditorModeBuilder : TileBuilderBase
    {
        private readonly Vector3 _tileScaleInEditor = new(0.95f, 0.95f, 0.95f);
        
        public EditorModeBuilder(MapBuilder mapBuilder) : base(mapBuilder)
        {
        }

        protected override void BuildNullTile(int floor, int row,int column)
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
                ? ObjectPool.Instance.SpawnFromPool(DefaultsProvider.defaultNullCubePrefab, LayoutParent.gameObject)
                : GameObject.CreatePrimitive(PrimitiveType.Cube);
            
            cube.transform.parent = LayoutParent;
            cube.transform.position = new Vector3(row, 0 - floor, column);
            cube.transform.localScale = _tileScaleInEditor;

            NullTile script = cube.GetComponent<NullTile>();
            
            if (MapBuilder.ShouldBeInvisible(floor))
            {
                script.ShowTile(false);
            }

            if (!NullTiles.ContainsKey(floor)) NullTiles.Add(floor, new List<NullTile>());
            NullTiles[floor].Add(script);
            
            PhysicalTiles.Add(cube.transform.position.ToVector3Int(), cube);
        }

        protected override void BuildNormalTile(int floor, int row, int column, TileDescription tileDescription)
        {
            base.BuildNormalTile(floor, row, column, tileDescription);

            LastBuiltTile.HideWall(TileDescription.ETileDirection.Ceiling);

            SetDisabledIfApplicable(LastBuiltTile.gameObject, floor);
        }

        private void SetDisabledIfApplicable(GameObject tile, int floor)
        {
            if (MapBuilder.ShouldBeInvisible(floor))
            {
                tile.SetActive(false);
            }
        }
    }
}