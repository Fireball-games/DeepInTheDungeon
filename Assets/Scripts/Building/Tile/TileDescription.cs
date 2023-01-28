using System;
using Scripts.Helpers.Extensions;
using UnityEngine;

namespace Scripts.Building.Tile
{
    public class TileDescription : ICloneable
    {
        public enum ETileDirection
        {
            Floor = 1,
            Ceiling = 2,
            North = 3,
            East = 4,
            South = 5,
            West = 6
        }

        public TileWalls TileWalls;
        public bool IsForMovement;

        public TileDescription()
        {
            TileWalls = new TileWalls();
        }

        public WallDescription GetWall(ETileDirection direction) => direction switch
        {
            ETileDirection.Floor => TileWalls.Floor,
            ETileDirection.Ceiling => TileWalls.Ceiling,
            ETileDirection.North => TileWalls.North,
            ETileDirection.East => TileWalls.East,
            ETileDirection.South => TileWalls.South,
            ETileDirection.West => TileWalls.West,
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };

        public WallDescription GetWall(Vector3Int direction)
        {
            if (direction == GeneralExtensions.WorldDown) return TileWalls.Floor;
            if (direction == GeneralExtensions.WorldUp) return TileWalls.Ceiling;
            if (direction == GeneralExtensions.WorldNorth) return TileWalls.North;
            if (direction == GeneralExtensions.WorldEast) return TileWalls.East;
            if (direction == GeneralExtensions.WorldSouth) return TileWalls.South;
            if (direction == GeneralExtensions.WorldWest) return TileWalls.West;
            return null;
        }

        public static TileDescription GetByLayout(int floor, int row, int column, TileDescription[,,] layout)
        {
            return new TileDescription
            {
                IsForMovement = true,
                TileWalls = new TileWalls
                {
                    Ceiling = new WallDescription(),
                    Floor = new WallDescription(),
                    North = WallForDirection(floor, row, column, GeneralExtensions.WorldNorth, layout),
                    East = WallForDirection(floor, row, column, GeneralExtensions.WorldEast, layout),
                    South = WallForDirection(floor, row, column, GeneralExtensions.WorldSouth, layout),
                    West = WallForDirection(floor, row, column, GeneralExtensions.WorldWest, layout),
                }
            };
        }

        private static WallDescription WallForDirection(int floor, int row, int column, Vector3Int direction, TileDescription[,,] layout)
        {
            return layout[floor + direction.x, row + direction.y, column + direction.z] == null ? new WallDescription() : null;
        }

        public object Clone() => new TileDescription
        {
            TileWalls = (TileWalls) TileWalls.Clone(),
            IsForMovement = IsForMovement
        };
    }
}