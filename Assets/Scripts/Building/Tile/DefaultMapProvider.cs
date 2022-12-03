using UnityEngine;

namespace Scripts.Building.Tile
{
    public static class DefaultMapProvider
    {
        public static TileDescription[,] Layout => new[,]
        {
            {null, null        , null      , null      , null        , null},
            {null, nwCornerWall, northWall , northWall , neCornerWall, null},
            {null, westWall    , centerTile, centerTile, eastWall    , null},
            {null, swCornerWall, southWall , southWall , seCornerWall, null},
            {null, null        , null      , null      , null        , null},
        };

        public static Vector3Int StartPosition => new(2, 0, 2);

        private static readonly TileDescription centerTile = new()
        {
            IsForMovement = true,
            Walls = new Walls
            {
                Floor = new WallDescription(),
                Ceiling = new WallDescription()
            }
        };
        
        private static readonly TileDescription northWall = new()
        {
            IsForMovement = true,
            Walls = new Walls
            {
                Floor = new WallDescription(),
                Ceiling = new WallDescription(),
                North = new WallDescription()
            }
        };
        
        private static readonly TileDescription southWall = new()
        {
            IsForMovement = true,
            Walls = new Walls
            {
                Floor = new WallDescription(),
                Ceiling = new WallDescription(),
                South = new WallDescription()
            }
        };
        
        private static readonly TileDescription eastWall = new()
        {
            IsForMovement = true,
            Walls = new Walls
            {
                Floor = new WallDescription(),
                Ceiling = new WallDescription(),
                East = new WallDescription()
            }
        };
        
        private static readonly TileDescription westWall = new()
        {
            IsForMovement = true,
            Walls = new Walls
            {
                Floor = new WallDescription(),
                Ceiling = new WallDescription(),
                West = new WallDescription()
            }
        };
        
        private static readonly TileDescription nwCornerWall = new()
        {
            IsForMovement = true,
            Walls = new Walls
            {
                Floor = new WallDescription(),
                Ceiling = new WallDescription(),
                North = new WallDescription(),
                West = new WallDescription(),
            }
        };
        
        private static readonly TileDescription neCornerWall = new()
        {
            IsForMovement = true,
            Walls = new Walls
            {
                Floor = new WallDescription(),
                Ceiling = new WallDescription(),
                North = new WallDescription(),
                East = new WallDescription(),
            }
        };
        
        private static readonly TileDescription swCornerWall = new()
        {
            IsForMovement = true,
            Walls = new Walls
            {
                Floor = new WallDescription(),
                Ceiling = new WallDescription(),
                South = new WallDescription(),
                West = new WallDescription(),
            }
        };
        
        private static readonly TileDescription seCornerWall = new()
        {
            IsForMovement = true,
            Walls = new Walls
            {
                Floor = new WallDescription(),
                Ceiling = new WallDescription(),
                South = new WallDescription(),
                East = new WallDescription(),
            }
        };
    }
}