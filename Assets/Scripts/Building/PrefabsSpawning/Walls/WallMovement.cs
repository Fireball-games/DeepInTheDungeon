using System;
using System.Collections.Generic;
using System.Linq;
using Scripts.Building.PrefabsSpawning.Walls.Identifications;
using Scripts.ScriptableObjects;
using UnityEngine;

namespace Scripts.Building.PrefabsSpawning.Walls
{
    [SelectionBase]
    public class WallMovement : WallPrefabBase, IMovementWall
    {
        public List<EditorWaypoint> presetWaypoints;

        public IEnumerable<Waypoint> GetWaypoints() => presetWaypoints.Select(wp => wp.ToWaypoint());
        
        [Serializable]
        public class EditorWaypoint
        {
            public Transform position;
            /// <summary>
            /// How is modified move speed while going towards this point. Multiplier, so 0.5 means half of default speed.
            /// </summary>
            public float moveSpeedModifier = 0.3f;
            
            public Waypoint ToWaypoint() => new(position.position, moveSpeedModifier);
        }
    }
}