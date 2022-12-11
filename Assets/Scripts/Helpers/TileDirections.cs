using System.Collections.Generic;
using UnityEngine;
using static Scripts.Building.Tile.TileDescription;
using static Scripts.Helpers.GeneralExtensions;

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
                GridUp,
                GridDown,
                GridNorth,
                GridEast,
                GridSouth,
                GridWest
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
                {GridUp, ETileDirection.Ceiling},
                {GridDown, ETileDirection.Floor},
                {GridNorth, ETileDirection.North},
                {GridEast, ETileDirection.East},
                {GridSouth, ETileDirection.South},
                {GridWest, ETileDirection.West}
            };

            VectorByTileDirection = new Dictionary<ETileDirection, Vector3Int>
            {
                {ETileDirection.Ceiling, GridUp},
                {ETileDirection.Floor, GridDown},
                {ETileDirection.North, GridNorth},
                {ETileDirection.East, GridEast},
                {ETileDirection.South, GridSouth},
                {ETileDirection.West, GridWest}
            };
        }
    }
}