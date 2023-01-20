using System.Collections.Generic;
using System.Linq;
using Scripts.ScriptableObjects;

namespace Scripts.Building.PrefabsSpawning.Configurations
{
    public class WallConfiguration : PrefabConfiguration
    {
        public float Offset;
        public List<Waypoint> WayPoints;

        public WallConfiguration()
        {
        }
        
        public WallConfiguration(WallConfiguration configuration) : base(configuration)
        {
            Offset = configuration.Offset;

            WayPoints = new List<Waypoint>();
        }

        public bool HasPath() => WayPoints != null && WayPoints.Any();
    }
}