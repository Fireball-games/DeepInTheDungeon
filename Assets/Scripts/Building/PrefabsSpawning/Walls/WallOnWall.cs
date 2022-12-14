using static Scripts.MapEditor.Enums;

namespace Scripts.Building.PrefabsSpawning.Walls
{
    public class WallOnWall : WallPrefabBaseBase
    {
        public override EWallType GetWallType() => EWallType.OnWall;
    }
}
