using UnityEngine;
using static Scripts.MapEditor.Enums;

namespace Scripts.Building.PrefabsSpawning.Walls
{
    public class WallBetween : WallPrefabBase
    {
        public override EWallType GetWallType() => EWallType.Between;
    }
}