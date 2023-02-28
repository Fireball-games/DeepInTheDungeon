using DG.Tweening;
using UnityEngine;

namespace Scripts.System.MonoBases
{
    public class UIElementBase : MonoBehaviour
    {
        [SerializeField] protected GameObject body;
        [SerializeField] private ETransitionType transitionType = ETransitionType.None;
        [SerializeField] private float transitionDuration = 0.5f;

        private CanvasGroup _canvasGroup;
        private RectTransform _rectTransform;
        private bool _isCollapsed;
        
        private enum ETransitionType
        {
            None,
            Fade,
            Scale
        }
        
        /// <summary>
        /// Disables body.
        /// </summary>
        /// <param name="isActive"></param>
        public virtual void SetActive(bool isActive)
        {
            if (!body) return;
            
            if (isActive && _isCollapsed)
            {
                SetCollapsed(false);
            }
            
            if (transitionType == ETransitionType.Fade)
            {
                _canvasGroup ??= gameObject.AddComponent<CanvasGroup>();
                _canvasGroup.alpha = isActive ? 0 : 1;
                body.SetActive(isActive);
                _canvasGroup.DOFade(isActive ? 1 : 0, transitionDuration).SetAutoKill(true).Play();
            }
            else if (transitionType == ETransitionType.Scale)
            {
                _rectTransform ??= gameObject.GetComponent<RectTransform>();
                _rectTransform.localScale = isActive ? Vector3.zero : Vector3.one;
                body.SetActive(isActive);
                _rectTransform.DOScale(isActive ? Vector3.one : Vector3.zero, transitionDuration).SetAutoKill(true).Play();
            }
            else
            {
                body.SetActive(isActive);
            }
        }

        /// <summary>
        /// Same as SetActive, but disables whole element, not just body.
        /// </summary>
        /// <param name="isCollapsed"></param>
        public void SetCollapsed(bool isCollapsed)
        {
            if (isCollapsed)
            {
                _isCollapsed = true;
                gameObject.SetActive(false);
                SetActive(false);
            }
            else
            {
                _isCollapsed = false;
                gameObject.SetActive(true);
                SetActive(true);
            }
        }

        public bool IsActive => body.activeSelf;
    }
}