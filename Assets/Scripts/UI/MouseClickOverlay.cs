using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

//Fireball Games * * * PetrZavodny.com

namespace UI
{
    public class MouseClickOverlay : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
#pragma warning disable 649
        [SerializeField] public UnityEvent OnClick;
        [SerializeField] public UnityEvent OnMouseEnter;
        [SerializeField] public UnityEvent OnMouseLeave;
#pragma warning restore 649

        public void OnPointerClick(PointerEventData eventData)
        {
            OnClick?.Invoke();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            OnMouseEnter?.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnMouseLeave?.Invoke();
        }
    }
}
