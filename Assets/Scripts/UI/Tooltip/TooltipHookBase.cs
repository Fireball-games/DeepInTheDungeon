using System.Collections;
using System.Collections.Generic;
using Scripts.UI.EditorUI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Scripts.UI.Tooltip
{
    public abstract class TooltipHookBase : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private const float DelayBeforeShow = 1f;
        private float _startTime;

        private bool _canShow;
        protected RectTransform OwnerTransform;
        private static TooltipController Tooltip => EditorUIManager.Instance.Tooltip;

        protected virtual void Awake()
        {
            OwnerTransform = GetComponent<RectTransform>();
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
            
            Tooltip.Show(OwnerTransform, GetTooltipStrings());
        }
    }
}