using System.Collections.Generic;
using Scripts.ScriptableObjects;
using UnityEngine;

namespace Scripts.MapEditor
{
    public class PathController : MonoBehaviour
    {
        public Dictionary<GameObject, WaypointParts> Waypoints;

        private void Awake()
        {
            Waypoints = new Dictionary<GameObject, WaypointParts>();
        }
    }
}