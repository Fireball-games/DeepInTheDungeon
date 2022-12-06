using System;
using Scripts.Building.Tile;
using UnityEngine;
using NotImplementedException = System.NotImplementedException;

namespace Scripts.Building
{
    public class MapDescription
    {
        public Vector3Int StartPosition { get; set; }
        public TileDescription[,] Layout;
        public string MapName = "DefaultMapName";

        public MapDescription()
        {
            StartPosition = DefaultMapProvider.StartPosition;
        }
    }
}
