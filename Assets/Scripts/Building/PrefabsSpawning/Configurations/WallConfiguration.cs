using System.Collections.Generic;
using Scripts.System;
using UnityEngine;

namespace Scripts.Building.PrefabsSpawning.Configurations
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