using UnityEngine;

namespace Scripts.Building.Walls
{
    public abstract class PrefabBase
    {
        public string PrefabName { get; set; }
        public Vector3 PositionOnMap { get; set; }
        public Vector3 Rotation { get; set; }
    }
}