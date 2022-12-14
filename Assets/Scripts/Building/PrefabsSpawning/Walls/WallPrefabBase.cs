using System;
using System.Collections.Generic;
using Scripts.System;
using UnityEngine;
using static Scripts.MapEditor.Enums;

namespace Scripts.Building.PrefabsSpawning.Walls
{
    public abstract class WallPrefabBase : MonoBehaviour
    {
        public abstract EWallType GetWallType();
        public List<Vector3> waypoints;
        [NonSerialized] public string PrefabName;
        [NonSerialized] public PositionRotation TransformData;
    }
}