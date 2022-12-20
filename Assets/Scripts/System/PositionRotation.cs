using System;
using UnityEngine;

namespace Scripts.System
{
    public class PositionRotation
    {
        protected bool Equals(PositionRotation other)
        {
            return Position.Equals(other.Position) && Rotation.Equals(other.Rotation);
        }

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

        public static bool operator == (PositionRotation a, PositionRotation b)
        {
            return a.Position == b.Position && a.Rotation == b.Rotation;
        }
        
        public static bool operator != (PositionRotation a, PositionRotation b)
        {
            return !(a == b);
        }
        
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PositionRotation) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Position, Rotation);
        }
    }
}