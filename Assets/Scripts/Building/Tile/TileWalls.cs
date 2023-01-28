using System;

namespace Scripts.Building.Tile
{
    public class TileWalls : ICloneable
    {
        public WallDescription Floor;
        public WallDescription Ceiling;
        public WallDescription North;
        public WallDescription East;
        public WallDescription South;
        public WallDescription West;

        public object Clone() => new TileWalls
        {
            Floor = (WallDescription) Floor?.Clone(),
            Ceiling = (WallDescription) Ceiling?.Clone(),
            North = (WallDescription) North?.Clone(),
            East = (WallDescription) East?.Clone(),
            South = (WallDescription) South?.Clone(),
            West = (WallDescription) West?.Clone()
        };
    }
}