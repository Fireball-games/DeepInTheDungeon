using UnityEngine;
using static Scripts.Building.Tile.TileDescription;

namespace Scripts.MapEditor
{
    public class WallGizmo : MonoBehaviour
    {
        public ETileDirection direction;
        public WallGizmoController controller;

        public void SetActive(bool isActive) => gameObject.SetActive(isActive);
        
        // private void OnMouseEnter()
        // {
        //     controller.OnGizmoEntered(direction);
        // }
        //
        // private void OnMouseExit()
        // {
        //     controller.OnGizmoExited();
        // }
    }
}
