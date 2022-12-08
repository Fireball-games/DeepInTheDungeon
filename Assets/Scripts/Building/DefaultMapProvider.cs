using Scripts.Building.Tile;
using UnityEngine;

namespace Scripts.Building
{
    public static class DefaultMapProvider
    {
        public static TileDescription[,,] Layout => new[,,]
        {
            {
                {null, null, null, null, null, null},
                {null, null, null, null, null, null},
                {null, null, null, null, null, null},
                {null, null, null, null, null, null},
                {null, null, null, null, null, null}
            },
            {
                {null, null, null, null, null, null},
                {null, fullTile, fullTile, fullTile, fullTile, null},
                {null, fullTile, fullTile, fullTile, fullTile, null},
                {null, fullTile, fullTile, fullTile, fullTile, null},
                {null, null, null, null, null, null}
            },
            {
                {null, null, null, null, null, null},
                {null, null, null, null, null, null},
                {null, null, null, null, null, null},
                {null, null, null, null, null, null},
                {null, null, null, null, null, null}
            }
        };

    public static TileDescription FullTile => fullTile;

        public static Vector3Int StartPosition => new(2, 0, 2);

        private static readonly TileDescription fullTile = new()
        {
            IsForMovement = true,
            Walls = new Walls
            {
                Floor = new WallDescription(),
                Ceiling = new WallDescription(),
                North = new WallDescription(),
                South = new WallDescription(),
                East = new WallDescription(),
                West = new WallDescription(),
            }
        };
    }
}