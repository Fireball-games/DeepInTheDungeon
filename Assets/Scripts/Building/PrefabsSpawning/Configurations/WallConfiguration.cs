using System.Collections.Generic;
using Scripts.System;
using UnityEngine;
using static Scripts.MapEditor.Enums;

namespace Scripts.Building.Walls.Configurations
{
    public class WallConfiguration : PrefabConfiguration
    {
        public EWallType WallType;
        public float Offset;
        public List<Vector3> WayPoints;

        public WallConfiguration()
        {
        }
        
        public WallConfiguration(WallConfiguration configuration)
        {
            WallType = configuration.WallType;
            Offset = configuration.Offset;
            WayPoints = configuration.WayPoints;

            PrefabName = configuration.PrefabName;
            TransformData = new PositionRotation(configuration.TransformData.Position, configuration.TransformData.Rotation);
        }

    }
}