using System;
using UnityEngine;

namespace Scripts.ScriptableObjects
{
    [Serializable]
    public class Waypoint
    {
        public Vector3 position;
        /// <summary>
        /// How is modified move speed while going towards this point. Multiplier, so 0.5 means half of default speed.
        /// </summary>
        public float moveSpeedModifier = 1f;
        
        public Waypoint(){}

        public Waypoint(Vector3 position, float moveSpeedModifier)
        {
            this.position = position;
            this.moveSpeedModifier = moveSpeedModifier;
        }

        public Waypoint(Waypoint source)
        {
            position = source.position;
            moveSpeedModifier = source.moveSpeedModifier;
        }
    }
}