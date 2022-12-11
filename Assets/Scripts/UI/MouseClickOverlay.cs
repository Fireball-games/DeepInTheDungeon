using System;
using UnityEngine;
using UnityEngine.EventSystems;

//Fireball Games * * * PetrZavodny.com

namespace Scripts.UI
{
    public class MouseClickOverlay : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public event Action OnClick;
        public event Action  OnMouseEnter;
        public event Action  OnMouseLeave;

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
