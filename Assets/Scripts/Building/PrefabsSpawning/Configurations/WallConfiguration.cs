using System.Collections.Generic;
using System.Linq;
using Scripts.ScriptableObjects;
using Scripts.System;

namespace Scripts.Building.PrefabsSpawning.Configurations
{
    public class WallConfiguration : PrefabConfiguration
    {
        public float Offset;
        public List<Waypoint> WayPoints;

        public WallConfiguration()
        {
        }
        
        public WallConfiguration(WallConfiguration configuration)
        {
            PrefabType = configuration.PrefabType;
            Offset = configuration.Offset;
            WayPoints = Waypoint.Clone(configuration.WayPoints).ToList();

            PrefabName = configuration.PrefabName;
            TransformData = new PositionRotation(configuration.TransformData.Position, configuration.TransformData.Rotation);
        }

    }
}