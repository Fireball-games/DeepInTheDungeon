using Scripts.Building.PrefabsSpawning.Walls.Identifications;
using Scripts.ScriptableObjects;
using UnityEngine;

namespace Scripts.Building.PrefabsSpawning.Walls
{
    [SelectionBase]
    public class WallMovement : WallPrefabBase, IMovementWall
    {
        public WaypointsPreset waypointsPreset;

        public WaypointsPreset GetWaypointPreset() => waypointsPreset;
    }
}