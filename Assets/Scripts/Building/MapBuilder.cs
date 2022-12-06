﻿using System;
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

        public void SetLayout(TileDescription[,] layout) => Layout = layout;

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

        /// <summary>
        /// Build a new tile where was previously null tile. Or null tile where was previously a tile.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <param name="layout"></param>
        public void RebuildTile(int row, int column)
        {
            if (GameController.Instance.GameMode is GameController.EGameMode.Play)
            {
                _playBuilder.BuildTile( row, column);
            }
            else
            {
                _editorBuilder.BuildTile(row, column);
            }
        }

        public void RegenerateTilesAround(int row, int column)
        {
            foreach (Vector3Int direction in TileDirections.VectorDirections)
            {
                if (Layout[row + direction.x,column + direction.z] != null)
                {
                    RegenerateTile(row + direction.x, column + direction.z);
                }
            }
        }
        
        /// <summary>
        /// Works over physical tile, shows or hides walls after assumed changed layout. 
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <param name="layout"></param>
        private void RegenerateTile(int row, int column)
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
                if (Layout[row + direction.x,column + direction.z] == null)
                    tileController.ShowWall(TileDirections.WallDirectionByVector[direction]);
                else
                    tileController.HideWall(TileDirections.WallDirectionByVector[direction]);
            }
        }
    }
}