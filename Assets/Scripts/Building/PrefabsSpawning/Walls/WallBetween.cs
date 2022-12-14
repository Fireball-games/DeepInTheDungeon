using UnityEngine;
using static Scripts.MapEditor.Enums;

namespace Scripts.Building.PrefabsSpawning.Walls
{
    public class WallBetween : WallPrefabBase
    {
        [Range(-0.5f, 0.5f)]public float offset;
        
        public override EWallType GetWallType() => EWallType.Between;
    }
}