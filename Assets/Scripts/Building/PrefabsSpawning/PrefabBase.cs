using UnityEngine;

namespace Scripts.Building.Walls
{
    public abstract class PrefabBase : MonoBehaviour
    {
        public string prefabName;
        public Vector3 positionOnMap;
        public Vector3 rotation;
    }
}