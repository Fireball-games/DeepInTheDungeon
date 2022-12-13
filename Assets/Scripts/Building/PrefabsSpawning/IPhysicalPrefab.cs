using UnityEngine;

namespace Scripts.Building.Walls
{
    public interface IPhysicalPrefab : IPrefab
    {
        public Vector3 prefabPosition { get; set; }
    }
}