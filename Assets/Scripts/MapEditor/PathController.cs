using System.Collections.Generic;
using Scripts.MapEditor.Services;
using UnityEngine;

namespace Scripts.MapEditor
{
    public class PathController : MonoBehaviour
    {
        public PathsService.EPathsType typeOfPath;
        public Dictionary<GameObject, WaypointParts> Waypoints;
        public bool isHighlighted;

        private void Awake()
        {
            Waypoints = new Dictionary<GameObject, WaypointParts>();
        }
    }
}