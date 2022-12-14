using System;
using System.Collections.Generic;
using UnityEngine;
using static Scripts.MapEditor.Enums;

namespace Scripts.Building.PrefabsSpawning.Walls
{
    public abstract class WallPrefabBase : MonoBehaviour
    {
        public abstract EWallType GetWallType();
        public List<Vector3> waypoints;
        [NonSerialized] public string PrefabName;
        [NonSerialized] public Vector3 PositionOnMap;
        [NonSerialized] public Vector3 Rotation;
    }
}