using System.Collections.Generic;
using System.Linq;
using Scripts.Building.PrefabsSpawning.Walls;
using Scripts.Building.PrefabsSpawning.Walls.Identifications;
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
        
        public WallConfiguration(WallPrefabBase wall, string ownerGuid = null, bool spawnPrefabOnBuild = true) : base(wall, ownerGuid, spawnPrefabOnBuild)
        {
            if (wall is WallBasic wallBasic)
            {
                Offset = wallBasic.offset;
            }

            if (wall is IMovementWall movementWall)
            {
                WayPoints = movementWall.GetWaypointPreset().ToList();
            }
        }

        public bool HasPath() => WayPoints != null && WayPoints.Any();
    }
}