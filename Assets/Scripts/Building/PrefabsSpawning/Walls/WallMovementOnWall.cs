using Scripts.Building.PrefabsSpawning.Walls.Indentificators;
using Scripts.ScriptableObjects;

namespace Scripts.Building.PrefabsSpawning.Walls
{
    public class WallMovementOnWall : WallOnWall, IMovementWall
    {
        public WaypointsPreset waypointsPreset;

        public WaypointsPreset GetWaypointPreset() => waypointsPreset;
    }
}