using System.Collections.Generic;
using UnityEngine;
using static Scripts.Building.Tile.TileDescription;

namespace Scripts.Helpers
{
    public static class TileDirections
    {
        public static IEnumerable<Vector3Int> Directions { get; }

        public static Dictionary<Vector3Int, ETileDirection> WallDirectionMap { get; }

        static TileDirections()
        {
            Directions = new[]
            {
                Extensions.Vector3IntUp,
                Extensions.Vector3IntDown,
                Extensions.Vector3IntNorth,
                Extensions.Vector3IntEast,
                Extensions.Vector3IntSouth,
                Extensions.Vector3IntWest
            };

            WallDirectionMap = new Dictionary<Vector3Int, ETileDirection>
            {
                {Extensions.Vector3IntUp, ETileDirection.Ceiling},
                {Extensions.Vector3IntDown, ETileDirection.Floor},
                {Extensions.Vector3IntNorth, ETileDirection.North},
                {Extensions.Vector3IntEast, ETileDirection.East},
                {Extensions.Vector3IntSouth, ETileDirection.South},
                {Extensions.Vector3IntWest, ETileDirection.West}
            };
        }
    }
}