using System;
using UnityEngine;
using UnityEngine.EventSystems;
using static Scripts.Building.Tile.TileDescription;

namespace Scripts.MapEditor
{
    public class WallGizmo : MonoBehaviour
    {
        public ETileDirection direction;
        public WallGizmoController controller;

        private void OnMouseEnter()
        {
            controller.OnGizmoExited();
        }

        private void OnMouseExit()
        {
            controller.OnGizmoEntered(direction);
        }
    }
}
