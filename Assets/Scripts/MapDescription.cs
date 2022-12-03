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
        
        
        private List<List<TileDescription>> _layout = new()
        {
            new List<TileDescription> {o, o, o, o, o, o, o, o, o, o, o},
            new List<TileDescription> {o, l, l, l, l, l, l, l, o, o, o},
            new List<TileDescription> {o, l, o, l, o, l, o, l, l, l, o},
            new List<TileDescription> {o, l, l, l, l, l, o, l, l, l, o},
            new List<TileDescription> {o, l, o, l, o, l, o, l, l, l, o},
            new List<TileDescription> {o, l, l, l, l, l, l, l, l, o, o},
            new List<TileDescription> {o, o, o, o, o, o, o, o, o, o, o},
        };
    
        private Vector3Int _startPosition = new(1, 0,2);

        public List<List<TileDescription>> Layout => _layout;
        public Vector3Int StartPosition => _startPosition;
    }
}
