using System.Collections.Generic;
using UnityEngine;
using static Scripts.Building.Tile.TileDescription;
using static Scripts.Helpers.Extensions.GeneralExtensions;

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
                WorldUp,
                WorldDown,
                WorldNorth,
                WorldEast,
                WorldSouth,
                WorldWest
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
                {WorldUp, ETileDirection.Ceiling},
                {WorldDown, ETileDirection.Floor},
                {WorldNorth, ETileDirection.North},
                {WorldEast, ETileDirection.East},
                {WorldSouth, ETileDirection.South},
                {WorldWest, ETileDirection.West}
            };

            VectorByTileDirection = new Dictionary<ETileDirection, Vector3Int>
            {
                {ETileDirection.Ceiling, WorldUp},
                {ETileDirection.Floor, WorldDown},
                {ETileDirection.North, WorldNorth},
                {ETileDirection.East, WorldEast},
                {ETileDirection.South, WorldSouth},
                {ETileDirection.West, WorldWest}
            };
        }
    }
}