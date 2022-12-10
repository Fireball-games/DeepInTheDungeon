using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;
using static Scripts.Building.Tile.TileDescription;

namespace Scripts.Helpers
{
    public static class TileDirections
    {
        public static IEnumerable<Vector3Int> VectorDirections { get; }
        public static IEnumerable<Vector3Int> HorizontalGridDirections { get; }

        public static Dictionary<Vector3Int, ETileDirection> WallDirectionByVector { get; }
        public static Dictionary<ETileDirection, Vector3Int> VectorByTileDirection { get; }

        static TileDirections()
        {
            VectorDirections = new[]
            {
                Extensions.GridUp,
                Extensions.GridDown,
                Extensions.GridNorth,
                Extensions.GridEast,
                Extensions.GridSouth,
                Extensions.GridWest
            };

            HorizontalGridDirections = new[]
            {
                new Vector3Int(0, 1, 0),
                new Vector3Int(0, -1, 0),
                new Vector3Int(0, 0, 1),
                new Vector3Int(0, 0, -1),
                new Vector3Int(0, 1, 1),
                new Vector3Int(0, -1, 1),
                new Vector3Int(0, -1, -1),
                new Vector3Int(0, 1, -1),
            };

            WallDirectionByVector = new Dictionary<Vector3Int, ETileDirection>
            {
                {Extensions.GridUp, ETileDirection.Ceiling},
                {Extensions.GridDown, ETileDirection.Floor},
                {Extensions.GridNorth, ETileDirection.North},
                {Extensions.GridEast, ETileDirection.East},
                {Extensions.GridSouth, ETileDirection.South},
                {Extensions.GridWest, ETileDirection.West}
            };

            VectorByTileDirection = new Dictionary<ETileDirection, Vector3Int>
            {
                {ETileDirection.Ceiling, Extensions.GridUp},
                {ETileDirection.Floor, Extensions.GridDown},
                {ETileDirection.North, Extensions.GridNorth},
                {ETileDirection.East, Extensions.GridEast},
                {ETileDirection.South, Extensions.GridSouth},
                {ETileDirection.West, Extensions.GridWest}
            };
        }
    }
}