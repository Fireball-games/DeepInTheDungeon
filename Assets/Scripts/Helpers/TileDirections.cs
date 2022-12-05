using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;
using static Scripts.Building.Tile.TileDescription;

namespace Scripts.Helpers
{
    public static class TileDirections
    {
        public static IEnumerable<Vector3Int> VectorDirections { get; }

        public static Dictionary<Vector3Int, ETileDirection> WallDirectionByVector { get; }
        public static Dictionary<ETileDirection, Vector3Int> VectorByTileDirection { get; }

        static TileDirections()
        {
            VectorDirections = new[]
            {
                // TODO: these will be used once Layout will be 3D
                // Extensions.Vector3IntUp,
                // Extensions.Vector3IntDown,
                Extensions.Vector3IntNorth,
                Extensions.Vector3IntEast,
                Extensions.Vector3IntSouth,
                Extensions.Vector3IntWest
            };

            WallDirectionByVector = new Dictionary<Vector3Int, ETileDirection>
            {
                {Extensions.Vector3IntUp, ETileDirection.Ceiling},
                {Extensions.Vector3IntDown, ETileDirection.Floor},
                {Extensions.Vector3IntNorth, ETileDirection.North},
                {Extensions.Vector3IntEast, ETileDirection.East},
                {Extensions.Vector3IntSouth, ETileDirection.South},
                {Extensions.Vector3IntWest, ETileDirection.West}
            };

            VectorByTileDirection = new Dictionary<ETileDirection, Vector3Int>
            {
                {ETileDirection.Ceiling, Extensions.Vector3IntUp},
                {ETileDirection.Floor, Extensions.Vector3IntDown},
                {ETileDirection.North, Extensions.Vector3IntNorth},
                {ETileDirection.East, Extensions.Vector3IntEast},
                {ETileDirection.South, Extensions.Vector3IntSouth},
                {ETileDirection.West, Extensions.Vector3IntWest}
            };
        }
    }
}