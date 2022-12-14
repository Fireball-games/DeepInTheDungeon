using UnityEngine;
using static Scripts.MapEditor.Enums;

namespace Scripts.Building.Walls.Configurations
{
    public class WallConfiguration : PrefabConfiguration
    {
        public EWallType WallType;
        public float Offset;
        public Vector3[] WayPoints;
    }
}