using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Scripts.UI.Tooltip
{
    public abstract class TooltipHookBase : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private const float DelayBeforeShow = 1f;
        private RectTransform _rectTransform;
        private float _startTime;

        private bool _canShow;

        protected virtual void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _startTime = Time.time;
            _canShow = true;
            StartCoroutine(ShowTooltipCoroutine());
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _canShow = false;
            Tooltip.Hide();
        }
        
        protected abstract IEnumerable<string> GetTooltipStrings();

        private IEnumerator ShowTooltipCoroutine()
        {
            while (Time.time - _startTime < DelayBeforeShow)
            {
                if (!_canShow) yield break;
                yield return null;
            }
            
            Tooltip.Show(_rectTransform, GetTooltipStrings());
        }
    }
}