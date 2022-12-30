using Scripts.Building.PrefabsSpawning.Walls.Identifications;
using Scripts.ScriptableObjects;
using NotImplementedException = System.NotImplementedException;

namespace Scripts.Building.PrefabsSpawning.Walls
{
    public class WallMovementBetween : WallBetween, IMovementWall
    {
        public WaypointsPreset waypointsPreset;
        public WaypointsPreset GetWaypointPreset() => waypointsPreset;
    }
}