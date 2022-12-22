using System.Collections.Generic;
using Scripts.MapEditor.Services;
using UnityEngine;

namespace Scripts.MapEditor
{
    public class PathController : MonoBehaviour
    {
        public Dictionary<GameObject, WaypointParts> Waypoints;
        public bool IsHighlighted;

        private void Awake()
        {
            Waypoints = new Dictionary<GameObject, WaypointParts>();
        }
    }
}