using Scripts.Building.Walls;
using UnityEngine;
using static Scripts.MapEditor.Enums;

namespace Scripts.Building.PrefabsSpawning.Walls
{
    public abstract class WallPrefabBase : MonoBehaviour
    {
        public string PrefabName { get; set; }
        public Vector3 PositionOnMap { get; set; }
        public Vector3 Rotation { get; set; }

        public abstract EWallType GetWallType();
    }
}