using System;
using Scripts.Helpers;
using UnityEngine;

namespace Scripts.Building.Tile
{
    public class TileDescription
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

        public Walls Walls;
        public bool IsForMovement;

        public WallDescription GetWall(ETileDirection direction) => direction switch
        {
            ETileDirection.Floor => Walls.Floor,
            ETileDirection.Ceiling => Walls.Ceiling,
            ETileDirection.North => Walls.North,
            ETileDirection.East => Walls.East,
            ETileDirection.South => Walls.South,
            ETileDirection.West => Walls.West,
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };
        
        public WallDescription GetWall(Vector3Int direction)
        {
            if (direction == GeneralExtensions.GridDown) return Walls.Floor;
            if (direction == GeneralExtensions.GridUp) return Walls.Ceiling;
            if (direction == GeneralExtensions.GridNorth) return Walls.North;
            if (direction == GeneralExtensions.GridEast) return Walls.East;
            if (direction == GeneralExtensions.GridSouth) return Walls.South;
            if (direction == GeneralExtensions.GridWest) return Walls.West;
            return null;
        }

        public static TileDescription GetByLayout(int row, int column, TileDescription[,] layout)
        {
            return  new TileDescription
            {
                IsForMovement = true,
                Walls = new Walls
                {
                    Ceiling = new WallDescription(),
                    Floor = new WallDescription(),
                    North = WallForDirection(row, column, GeneralExtensions.GridNorth, layout),
                    East = WallForDirection(row, column, GeneralExtensions.GridEast, layout),
                    South = WallForDirection(row, column, GeneralExtensions.GridSouth, layout),
                    West = WallForDirection(row, column, GeneralExtensions.GridWest, layout),
                }
            };
        }

        private static WallDescription WallForDirection(int row, int column, Vector3Int direction, TileDescription[,] layout)
        {
            return layout[row + direction.x, column + direction.z] == null ? new WallDescription() : null;
        }
    }
}