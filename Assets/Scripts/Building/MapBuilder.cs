using System;
using System.Collections;
using System.Collections.Generic;
using Scripts.Building.Tile;
using Scripts.Helpers;
using Scripts.System;
using Scripts.System.Pooling;
using UnityEngine;
using LayoutType = System.Collections.Generic.List<System.Collections.Generic.List<Scripts.Building.Tile.TileDescription>>;
using Logger = Scripts.Helpers.Logger;

namespace Scripts.Building
{
    public class MapBuilder : MonoBehaviour
    {
        public DefaultBuildPartsProvider defaultsProvider;

        private TileBuilderBase _playBuilder;
        private TileBuilderBase _editorBuilder;

        public event Action OnLayoutBuilt;

        internal Transform LayoutParent;
        internal TileDescription[,,] Layout;
        internal Dictionary<Vector3Int, GameObject> PhysicalTiles;
        internal MapDescription MapDescription;

        private void Awake()
        {
            PhysicalTiles = new Dictionary<Vector3Int, GameObject>();
            
            if(!LayoutParent)
            {
                LayoutParent = new GameObject("Layout").transform;
                LayoutParent.transform.parent = transform;
            }
        }

        public void BuildMap(MapDescription mapDescription)
        {
            DemolishMap();

            MapDescription = mapDescription;
            
            StartCoroutine(BuildLayoutCoroutine(mapDescription.Layout));
        }

        public void SetLayout(TileDescription[,,] layout) => Layout = layout;

        private IEnumerator BuildLayoutCoroutine(TileDescription[,,] layout)
        {
            Layout = layout;
            
            _playBuilder = new PlayModeBuilder(this);
            _editorBuilder = new EditorModeBuilder(this);
            
            for (int floor = 0; floor < layout.GetLength(0); floor++)
            {
                for (int row = 0; row < layout.GetLength(1); row++)
                {
                    for (int column = 0; column < layout.GetLength(2); column++)
                    {
                        if (GameManager.Instance.GameMode is GameManager.EGameMode.Play)
                        {
                            _playBuilder.BuildTile(floor, row, column);
                        }
                        else
                        {
                            _editorBuilder.BuildTile(floor, row, column);
                        }
                    
                        yield return null;
                    }
                }
            }

            OnLayoutBuilt?.Invoke();
        }

        public void DemolishMap()
        {
            foreach (GameObject tile in PhysicalTiles.Values)
            {
                ObjectPool.Instance.ReturnToPool(tile);
            }
            
            PhysicalTiles.Clear();
        }

        /// <summary>
        /// Build a new tile where was previously null tile. Or null tile where was previously a tile.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <param name="floor"></param>
        public void RebuildTile(int floor, int row, int column)
        {
            if (GameManager.Instance.GameMode is GameManager.EGameMode.Play)
            {
                _playBuilder.BuildTile( floor, row, column);
            }
            else
            {
                _editorBuilder.BuildTile(floor, row, column);
            }
        }

        public void RegenerateTilesAround(int floor, int row, int column)
        {
            foreach (Vector3Int direction in TileDirections.VectorDirections)
            {
                if (Layout[floor + direction.y, row + direction.x, column + direction.z] != null)
                {
                    RegenerateTile(floor + direction.y, row + direction.x, column + direction.z);
                }
            }
        }

        /// <summary>
        /// Works over physical tile, shows or hides walls after assumed changed layout. 
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <param name="floor"></param>
        private void RegenerateTile(int floor, int row, int column)
        {
            Vector3Int key = new (row, 0, column);
            TileController tileController = PhysicalTiles[key].GetComponent<TileController>();

            if (!tileController)
            {
                Logger.LogWarning($"Attempt to regenerate tile which is null tile on position: [{row}][{column}]");
                return;
            }
            
            foreach (Vector3Int direction in TileDirections.VectorDirections)
            {
                if (Layout[row + direction.x, floor + direction.y, column + direction.z] == null)
                    tileController.ShowWall(TileDirections.WallDirectionByVector[direction]);
                else
                    tileController.HideWall(TileDirections.WallDirectionByVector[direction]);
            }
        }

        public static MapDescription GenerateDefaultMap(int floors, int rows, int columns)
        {
            TileDescription[,,] layout = new TileDescription[floors, rows, columns];

            Vector3Int center = new(floors / 2, rows / 2, columns / 2);

            layout = AddTilesToCenterOfLayout(layout);

            return new MapDescription
            {
                Layout = layout,
                StartGridPosition = center,
            };
        }

        private static TileDescription[,,] AddTilesToCenterOfLayout(TileDescription[,,] layout)
        {
            Vector2Int center = new(layout.GetLength(1) / 2, layout.GetLength(2) / 2);
            int floor = layout.GetLength(0) / 2;

            layout[floor, center.x - 1, center.y - 1] = DefaultMapProvider.FullTile;
            layout[floor, center.x - 1, center.y + 1] = DefaultMapProvider.FullTile;
            layout[floor, center.x - 1, center.y] = DefaultMapProvider.FullTile;
            layout[floor, center.x , center.y - 1] = DefaultMapProvider.FullTile;
            layout[floor, center.x, center.y] = DefaultMapProvider.FullTile;
            layout[floor, center.x, center.y + 1] = DefaultMapProvider.FullTile;
            layout[floor, center.x + 1, center.y - 1] = DefaultMapProvider.FullTile;
            layout[floor, center.x + 1, center.y] = DefaultMapProvider.FullTile;
            layout[floor, center.x + 1, center.y + 1] = DefaultMapProvider.FullTile;

            return layout;
        }
    }
}