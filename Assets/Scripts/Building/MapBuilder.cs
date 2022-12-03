using System;
using System.Collections;
using System.Collections.Generic;
using Scripts.Building.Tile;
using UnityEngine;

namespace Scripts.Building
{
    public class MapBuilder : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject floorPrefab;
        [SerializeField] private GameObject ceilingPrefab;
        [SerializeField] private GameObject wallPrefab;

        private TileBuilderBase _playBuilder;
        private TileBuilderBase _editorBuilder;

        public event Action OnLayoutBuilt;

        internal Transform LayoutParent;
        internal List<List<TileDescription>> Layout;

        internal GameObject FloorPrefab => floorPrefab;
        internal GameObject CeilingPrefab => ceilingPrefab;
        internal GameObject WallPrefab => wallPrefab;

        private void Awake()
        {
            LayoutParent ??= new GameObject("Layout").transform;
        }

        public void BuildMap(MapDescription mapDescription)
        {
            StartCoroutine(BuildLayout(mapDescription.Layout));
        }

        private IEnumerator BuildLayout(List<List<TileDescription>> layout)
        {
            Layout = layout;
            
            _playBuilder ??= new PlayModeBuilder(this);
            _editorBuilder ??= new EditorModeBuilder(this);
            
            for (int x = 0; x < layout.Count; x++)
            {
                for (int y = 0; y < layout[0].Count; y++)
                {
                    if (GameController.GameMode is GameController.EGameMode.Play)
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
    }
}