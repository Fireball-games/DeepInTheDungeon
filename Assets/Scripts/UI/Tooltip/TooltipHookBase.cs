using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Scripts.UI.Tooltip
{
    public abstract class TooltipHookBase : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private static Tooltip _tooltip;
        private const float DelayBeforeShow = 1f;
        private float _startTime;

        private bool _canShow;
        protected RectTransform Owner;

        protected virtual void Awake()
        {
            Owner = GetComponent<RectTransform>();
            
            if (!_tooltip)
            {
                _tooltip = FindObjectOfType<Tooltip>();
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_tooltip) return;
            
            _startTime = Time.time;
            _canShow = true;
            StartCoroutine(ShowTooltipCoroutine());
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!_tooltip) return;
            
            _canShow = false;
            _tooltip.Hide();
        }
        
        protected abstract IEnumerable<string> GetTooltipStrings();

        private IEnumerator ShowTooltipCoroutine()
        {
            while (Time.time - _startTime < DelayBeforeShow)
            {
                if (!_canShow) yield break;
                yield return null;
            }
            
            _tooltip.Show(Owner, GetTooltipStrings());
        }
    }
}