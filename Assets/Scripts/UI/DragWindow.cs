﻿namespace Scripts.UI
{
    using UnityEngine;
    using UnityEngine.EventSystems;

    public class DragWindow : MonoBehaviour, IBeginDragHandler, IDragHandler
    {
        private RectTransform _rectTransform;
        private Rect _parentRect;
        private Vector2 _offsetToPointer;

        private void Start()
        {
            _rectTransform = GetComponent<RectTransform>();
            _parentRect = _rectTransform.parent.GetComponent<RectTransform>().rect;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _offsetToPointer = eventData.position - _rectTransform.anchoredPosition;
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector2 anchoredPosition = eventData.position - _offsetToPointer;

            float minX = -_rectTransform.anchorMin.x * _parentRect.width + _rectTransform.rect.width * (1 - _rectTransform.anchorMax.x);
            float maxX = _parentRect.width * (1 - _rectTransform.anchorMin.x) - _rectTransform.rect.width * _rectTransform.anchorMax.x;
            float minY = -_rectTransform.anchorMin.y * _parentRect.height + _rectTransform.rect.height * (1 - _rectTransform.anchorMax.y);
            float maxY = _parentRect.height * (1 - _rectTransform.anchorMin.y) - _rectTransform.rect.height * _rectTransform.anchorMax.y;

            anchoredPosition.x = Mathf.Clamp(anchoredPosition.x, minX, maxX);
            anchoredPosition.y = Mathf.Clamp(anchoredPosition.y, minY, maxY);

            _rectTransform.anchoredPosition = anchoredPosition;
        }
    }
}