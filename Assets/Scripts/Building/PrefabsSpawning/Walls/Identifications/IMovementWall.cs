using System.Collections.Generic;
using Scripts.ScriptableObjects;

namespace Scripts.Building.PrefabsSpawning.Walls.Identifications
{
    public interface IMovementWall
    {
        public IEnumerable<Waypoint> GetWaypointPreset();
    }
}