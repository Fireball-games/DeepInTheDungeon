using System;
using System.Collections;
using Scripts.Building.Tile;
using Scripts.System;
using UnityEngine;

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

        protected override void Awake()
        {
            base.Awake();
            
            if(!LayoutParent)
            {
                LayoutParent = new GameObject("Layout").transform;
                LayoutParent.transform.parent = transform;
            }
        }

        public void BuildMap(MapDescription mapDescription)
        {
            StartCoroutine(BuildLayout(mapDescription.Layout));
        }

        private IEnumerator BuildLayout(TileDescription[,] layout)
        {
            Layout = layout;
            
            _playBuilder ??= new PlayModeBuilder(this);
            _editorBuilder ??= new EditorModeBuilder(this);
            
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
    }
}