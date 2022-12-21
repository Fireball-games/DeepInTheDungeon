using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.MapEditor
{
    public class PathController : MonoBehaviour
    {
        public List<GameObject> waypoints;

        private void Awake()
        {
            waypoints = new List<GameObject>();
        }
    }
}