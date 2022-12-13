using UnityEngine;

namespace Scripts.Building.Walls
{
    public interface IPrefab
    {
        public string prefabName { get; set; }
        public Vector3 position { get; set; }
    }
}