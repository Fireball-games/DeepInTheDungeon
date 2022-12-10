﻿using System;
using Scripts.Helpers;
using UnityEditor;
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

        // public static ETileDirection[] TileDirections => new[]
        // {
        //     ETileDirection.Floor,
        //     ETileDirection.Ceiling,
        //     ETileDirection.North,
        //     ETileDirection.East,
        //     ETileDirection.South,
        //     ETileDirection.West
        // };
        
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
            // TODO: not sure if floor and ceiling should not be reversed
            if (direction == Extensions.GridDown) return Walls.Floor;
            if (direction == Extensions.GridUp) return Walls.Ceiling;
            if (direction == Extensions.GridNorth) return Walls.North;
            if (direction == Extensions.GridEast) return Walls.East;
            if (direction == Extensions.GridSouth) return Walls.South;
            if (direction == Extensions.GridWest) return Walls.West;
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
                    North = WallForDirection(row, column, Extensions.GridNorth, layout),
                    East = WallForDirection(row, column, Extensions.GridEast, layout),
                    South = WallForDirection(row, column, Extensions.GridSouth, layout),
                    West = WallForDirection(row, column, Extensions.GridWest, layout),
                }
            };
        }

        private static WallDescription WallForDirection(int row, int column, Vector3Int direction, TileDescription[,] layout)
        {
            return layout[row + direction.x, column + direction.z] == null ? new WallDescription() : null;
        }
    }
}