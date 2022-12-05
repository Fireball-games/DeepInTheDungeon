using System;
using System.Collections;
using System.Collections.Generic;
using Scripts.Building.Tile;
using Scripts.Helpers;
using Scripts.System;
using Scripts.System.Pooling;
using UnityEngine;
using LayoutType = System.Collections.Generic.List<System.Collections.Generic.List<Scripts.Building.Tile.TileDescription>>;

namespace Scripts.Building
{
    public class MapBuilder : InitializeFromResourceBase
    {
        public DefaultBuildPartsProvider defaultsProvider;

        private TileBuilderBase _playBuilder;
        private TileBuilderBase _editorBuilder;

        public event Action OnLayoutBuilt;

        internal Transform LayoutParent;
        internal TileDescription[,] Layout;
        internal Dictionary<Vector3Int, GameObject> PhysicalTiles;

        protected override void Awake()
        {
            base.Awake();
            PhysicalTiles = new Dictionary<Vector3Int, GameObject>();
            
            if(!LayoutParent)
            {
                LayoutParent = new GameObject("Layout").transform;
                LayoutParent.transform.parent = transform;
            }
        }

        public void BuildMap(MapDescription mapDescription)
        {
            StartCoroutine(BuildLayoutCoroutine(mapDescription.Layout));
        }

        private IEnumerator BuildLayoutCoroutine(TileDescription[,] layout)
        {
            Layout = layout;
            
            _playBuilder = new PlayModeBuilder(this);
            _editorBuilder = new EditorModeBuilder(this);
            
            for (int x = 0; x < layout.GetLength(0); x++)
            {
                for (int y = 0; y < layout.GetLength(1); y++)
                {
                    if (GameController.Instance.GameMode is GameController.EGameMode.Play)
                    {
                        _playBuilder.BuildTile( x, y);
                    }
                    else
                    {
                        _editorBuilder.BuildTile(x, y);
                    }
                    
                    yield return null;
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

        public void RegenerateTilesAround(int row, int column, LayoutType layout)
        {
            RegenerateTile(row, column, layout);

            foreach (Vector3Int direction in TileDirections.Directions)
            {
                RegenerateTile(row + direction.x, column + direction.y, layout);
            }
        }

        private void RegenerateTile(int row, int column, LayoutType layout)
        {
            TileDescription tile = layout[row][column];

            foreach (Vector3Int direction in TileDirections.Directions)
            {
                // if (layout[row + direction.x][column + direction.y] == null)
                //     tile.Walls.
            }
        }
    }
}