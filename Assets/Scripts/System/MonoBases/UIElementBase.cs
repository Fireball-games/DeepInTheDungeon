using System;
using System.Threading.Tasks;
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
        protected CanvasGroup CanvasGroup
        {
            get
            {
                if (!_canvasGroup)
                {
                    _canvasGroup = GetComponent<CanvasGroup>();
                    
                    if (!_canvasGroup)
                    {
                        _canvasGroup = gameObject.AddComponent<CanvasGroup>();
                    }
                }

                return _canvasGroup;
            }
        }
        
        private RectTransform _rectTransform;
        
        private RectTransform RectTransform
        {
            get
            {
                if (!_rectTransform)
                {
                    _rectTransform = GetComponent<RectTransform>();
                }

                return _rectTransform;
            }
        }
        
        private bool _isCollapsed;
        
        private enum ETransitionType
        {
            None,
            Fade,
            Scale
        }
        
        /// <summary>
        /// This method should be called if you want to set element to proper values before transition, like first show time. 
        /// </summary>
        public void PrepareForTransition()
        {
            if (!body) return;
            
            if (transitionType == ETransitionType.Fade)
            {
                CanvasGroup.alpha = 0;
            }
            else if (transitionType == ETransitionType.Scale)
            {
                RectTransform.localScale = Vector3.zero;
            }
        }

        /// <summary>
        /// Disables/Enables the body, IGNORES TRANSITIONS.
        /// </summary>
        /// <param name="isActive"></param>
        public virtual void SetActive(bool isActive)
        {
            if (!body) return;

            if (isActive && _isCollapsed)
            {
                SetCollapsed(false);
            }
            
            body.SetActive(isActive);
        }

        /// <summary>
        /// Disables body. Like SetActive, but works with transitions.
        /// </summary>
        /// <param name="isActive"></param>
        public virtual async Task SetActiveAsync(bool isActive)
        {
            if (!body) return;

            TaskCompletionSource<bool> tcs = new();

            if (isActive && _isCollapsed)
            {
                SetCollapsed(false);
            }
            
            if (transitionType == ETransitionType.Fade)
            {
                // If the body is already active/inactive, return.
                if (isActive && Math.Abs(CanvasGroup.alpha - 1) < float.Epsilon 
                    || !isActive && Math.Abs(CanvasGroup.alpha) < float.Epsilon)
                {
                    body.SetActive(isActive);
                    tcs.SetResult(true);
                    return;
                }
                
                body.SetActive(true);
                CanvasGroup.DOFade(isActive ? 1 : 0, transitionDuration).SetAutoKill(true).OnComplete(() =>
                {
                    tcs.SetResult(true);
                    body.SetActive(isActive);
                }).Play();
            }
            else if (transitionType == ETransitionType.Scale)
            {
                if (isActive && RectTransform.localScale == Vector3.one 
                    || !isActive && RectTransform.localScale == Vector3.zero)
                {
                    body.SetActive(isActive);
                    tcs.SetResult(true);
                    return;
                }
                
                body.SetActive(true);
                RectTransform.DOScale(isActive ? Vector3.one : Vector3.zero, transitionDuration).SetAutoKill(true)
                    .OnComplete(() =>
                    {
                        tcs.SetResult(true);
                        body.SetActive(isActive);
                    })
                    .Play();
            }
            else
            {
                tcs.SetResult(true);
                body.SetActive(isActive);
            }

            await tcs.Task;
        }
        
        public void SetCollapsed(bool isCollapsed)
        {
                if (isCollapsed)
                {
                    SetActive(false);
                    _isCollapsed = true;
                    gameObject.SetActive(false);
                }
                else
                {
                    _isCollapsed = false;
                    SetActive(true);
                    gameObject.SetActive(true);
                }
        }

        /// <summary>
        /// Same as SetActive, but disables whole element, not just body.
        /// </summary>
        /// <param name="isCollapsed"></param>
        public async Task SetCollapsedAsync(bool isCollapsed)
        {
            TaskCompletionSource<bool> tcs = new();
            
            if (isCollapsed)
            {
                await SetActiveAsync(false);
                _isCollapsed = true;
                gameObject.SetActive(false);
                tcs.SetResult(true);
            }
            else
            {
                gameObject.SetActive(true);
                await SetActiveAsync(true);
                _isCollapsed = false;
                tcs.SetResult(true);
            }
            
            await tcs.Task;
        }

        public bool IsActive => body.activeSelf;
    }
}