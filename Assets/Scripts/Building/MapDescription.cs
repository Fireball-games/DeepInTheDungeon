using System.Collections.Generic;
using Scripts.Building.Tile;
using UnityEngine;

namespace Scripts
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
        private Vector3Int _startPosition;

        public MapDescription()
        {
            _layout = DefaultMapProvider.Layout;
            _startPosition = DefaultMapProvider.StartPosition;
        }
        
        public TileDescription[,] Layout => _layout ?? DefaultMapProvider.Layout;
        
        public Vector3Int StartPosition => _startPosition;
    }
}
