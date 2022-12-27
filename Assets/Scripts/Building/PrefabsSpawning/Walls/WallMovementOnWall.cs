using Scripts.Building.PrefabsSpawning.Walls.Indentificators;
using Scripts.ScriptableObjects;
using UnityEngine;

namespace Scripts.Building.PrefabsSpawning.Walls
{
    [SelectionBase]
    public class WallMovementOnWall : WallOnWall, IMovementWall
    {
        public WaypointsPreset waypointsPreset;

        public WaypointsPreset GetWaypointPreset() => waypointsPreset;
    }
}