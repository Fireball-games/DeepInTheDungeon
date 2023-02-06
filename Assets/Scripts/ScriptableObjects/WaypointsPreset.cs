using System.Collections.Generic;
using UnityEngine;

namespace Scripts.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Configurations/Waypoints Preset", fileName = "WaypointPreset")]
    public class WaypointsPreset : ScriptableObject
    {
        public List<Transform> waypoints;
    }
}
