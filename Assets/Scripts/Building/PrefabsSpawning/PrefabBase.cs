using UnityEngine;
using static Scripts.Enums;

namespace Scripts.Building.Walls
{
    public abstract class PrefabBase : MonoBehaviour
    {
        public string prefabName;
        public Vector3 positionOnMap;
        public Vector3 rotation;
        public EPrefabType prefabType;
    }
}