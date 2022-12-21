using System;
using UnityEngine;

namespace Scripts.ScriptableObjects
{
    [Serializable]
    public class Waypoint
    {
        public Vector3 Position;
        /// <summary>
        /// How is modified move speed while going towards this point. Multiplier, so 0.5 means half of default speed.
        /// </summary>
        public float MoveSpeedModifier = 1f;
    }
}