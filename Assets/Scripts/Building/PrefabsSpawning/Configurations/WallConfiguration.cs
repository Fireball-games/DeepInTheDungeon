using System.Collections.Generic;
using UnityEngine;
using static Scripts.MapEditor.Enums;

namespace Scripts.Building.Walls.Configurations
{
    public class WallConfiguration : PrefabConfiguration
    {
        public EWallType WallType;
        public float Offset;
        public List<Vector3> WayPoints;
    }
}