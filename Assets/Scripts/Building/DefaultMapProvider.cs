using Scripts.Building.Tile;

namespace Scripts.Building
{
    public static class DefaultMapProvider
    {
        // ReSharper disable once UnusedMember.Global - left just for a demonstration purposes how looks basic map
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
                {null, FullTile, FullTile, FullTile, FullTile, null},
                {null, FullTile, FullTile, FullTile, FullTile, null},
                {null, FullTile, FullTile, FullTile, FullTile, null},
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

        public static TileDescription FullTile { get; } = new()
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