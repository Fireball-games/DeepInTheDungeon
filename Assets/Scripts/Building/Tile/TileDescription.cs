namespace Scripts.Building.Tile
{
    public class TileDescription
    {
        public Walls Walls;
        public bool IsForMovement;
    }

    public class Walls
    {
        public WallsDescription Floor;
        public WallsDescription Ceiling;
        public WallsDescription North;
        public WallsDescription East;
        public WallsDescription South;
        public WallsDescription West;
    }
}