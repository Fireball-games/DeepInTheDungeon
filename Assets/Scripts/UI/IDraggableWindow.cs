using UnityEngine.EventSystems;

namespace Scripts.UI
{
    public interface IDraggableWindow
    {
        void OnDragStart(PointerEventData eventData);
        void OnDragEnd(PointerEventData eventData);
    }
}