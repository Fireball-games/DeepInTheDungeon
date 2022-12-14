using UnityEngine;

namespace Scripts.System
{
    public class PositionRotation
    {
        public Vector3 Position;
        public Quaternion Rotation;
        
        public PositionRotation() : this(Vector3.zero, Quaternion.identity)
        {
        }

        public PositionRotation(Vector3 position, Quaternion rotation)
        {
            Position = position;
            Rotation = rotation;
        }
    }
}