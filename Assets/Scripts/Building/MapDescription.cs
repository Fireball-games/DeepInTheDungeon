using Scripts.Building.Tile;
using UnityEngine;
using NotImplementedException = System.NotImplementedException;

namespace Scripts.Building
{
    public class MapDescription
    {
        private static TileDescription o = new TileDescription()
        {
            IsForMovement = false
        };
        private static TileDescription l = new TileDescription()
        {
            IsForMovement = true
        };
        
        
        // private List<List<TileDescription>> _layout = new()
        // {
        //     new List<TileDescription> {o, o, o, o, o, o, o, o, o, o, o},
        //     new List<TileDescription> {o, l, l, l, l, l, l, l, o, o, o},
        //     new List<TileDescription> {o, l, o, l, o, l, o, l, l, l, o},
        //     new List<TileDescription> {o, l, l, l, l, l, o, l, l, l, o},
        //     new List<TileDescription> {o, l, o, l, o, l, o, l, l, l, o},
        //     new List<TileDescription> {o, l, l, l, l, l, l, l, l, o, o},
        //     new List<TileDescription> {o, o, o, o, o, o, o, o, o, o, o},
        // };
        private TileDescription[,] _layout;

        public MapDescription()
        {
            _layout = DefaultMapProvider.Layout;
            StartPosition = DefaultMapProvider.StartPosition;
        }
        
        public TileDescription[,] Layout
        {
            get => _layout ?? DefaultMapProvider.Layout;
            set => _layout = value;
        }

        public Vector3Int StartPosition { get; set; }
    }
}
