using System.Collections.Generic;
using Scripts.Building.PrefabsSpawning.Walls.Indentificators;
using Scripts.ScriptableObjects;

namespace Scripts.Building.PrefabsSpawning.Walls
{
    public class WallMovementBetween : WallBetween, IMovementWall
    {
        public List<Waypoint> waypoints;
        public WaypointsPreset waypointsPreset;
    }
}