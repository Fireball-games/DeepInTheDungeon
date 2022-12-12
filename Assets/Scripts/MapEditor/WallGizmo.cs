using UnityEngine;
using UnityEngine.EventSystems;
using static Scripts.Building.Tile.TileDescription;

namespace Scripts.MapEditor
{
    public class WallGizmo : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public ETileDirection direction;
        public WallGizmoController controller;
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            controller.OnGizmoEntered(direction);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            controller.OnGizmoExited();
        }
    }
}
