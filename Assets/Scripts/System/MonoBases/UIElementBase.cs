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
        private RectTransform _rectTransform;
        private bool _isCollapsed;
        
        private enum ETransitionType
        {
            None,
            Fade,
            Scale
        }

        private void Awake()
        {
            if (transitionType == ETransitionType.Scale)
            {
                _rectTransform = gameObject.GetComponent<RectTransform>();
                _rectTransform.localScale = Vector3.zero;
            }
            
            if (transitionType == ETransitionType.Fade)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
                _canvasGroup.alpha = 0;
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
                if (!_canvasGroup) Awake();
                body.SetActive(true);
                _canvasGroup.DOFade(isActive ? 1 : 0, transitionDuration).SetAutoKill(true).OnComplete(() =>
                {
                    tcs.SetResult(true);
                    body.SetActive(isActive);
                }).Play();
            }
            else if (transitionType == ETransitionType.Scale)
            {
                if (!_rectTransform) Awake();
                body.SetActive(true);
                _rectTransform.DOScale(isActive ? Vector3.one : Vector3.zero, transitionDuration).SetAutoKill(true)
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
                    gameObject.SetActive(true);
                    SetActive(true);
                    _isCollapsed = false;
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