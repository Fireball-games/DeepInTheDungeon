using System.Collections.Generic;
using UnityEngine;

namespace Scripts
{
    public class MapDescription
    {
        private List<List<int>> _layout = new()
        {
            new List<int> {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            new List<int> {0, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0},
            new List<int> {0, 1, 0, 1, 0, 1, 0, 1, 1, 1, 0},
            new List<int> {0, 1, 1, 1, 1, 1, 0, 1, 1, 1, 0},
            new List<int> {0, 1, 0, 1, 0, 1, 0, 1, 1, 1, 0},
            new List<int> {0, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0},
            new List<int> {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        };
    
        private Vector3Int _startPosition = new(1, 0,2);

        public List<List<int>> Layout => _layout;
        public Vector3Int StartPosition => _startPosition;
    }
}
