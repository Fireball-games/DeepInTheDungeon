using System.Collections.Generic;
using Scripts.Building.PrefabsSpawning.Configurations;
using Scripts.System;
using UnityEngine;
using static Scripts.MapEditor.Enums;

namespace Scripts.Building.Walls.Configurations
{
    public class WallConfiguration : PrefabConfiguration
    {
        public float Offset;
        public List<Vector3> WayPoints;

        public WallConfiguration()
        {
        }
        
        public WallConfiguration(WallConfiguration configuration)
        {
            PrefabType = configuration.PrefabType;
            Offset = configuration.Offset;
            WayPoints = configuration.WayPoints;

            PrefabName = configuration.PrefabName;
            TransformData = new PositionRotation(configuration.TransformData.Position, configuration.TransformData.Rotation);
        }

    }
}