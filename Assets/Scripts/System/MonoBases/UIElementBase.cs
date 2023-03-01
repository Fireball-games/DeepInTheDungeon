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

        /// <summary>
        /// Disables body.
        /// </summary>
        /// <param name="isActive"></param>
        public virtual async Task SetActive(bool isActive)
        {
            if (!body) return;

            TaskCompletionSource<bool> tcs = new();

            if (isActive && _isCollapsed)
            {
                await SetCollapsed(false);
            }
            
            if (transitionType == ETransitionType.Fade)
            {
                _canvasGroup ??= gameObject.AddComponent<CanvasGroup>();
                _canvasGroup.alpha = isActive ? 0 : 1;
                body.SetActive(true);
                _canvasGroup.DOFade(isActive ? 1 : 0, transitionDuration).SetAutoKill(true).OnComplete(() =>
                {
                    tcs.SetResult(true);
                    body.SetActive(isActive);
                }).Play();
            }
            else if (transitionType == ETransitionType.Scale)
            {
                _rectTransform ??= gameObject.GetComponent<RectTransform>();
                _rectTransform.localScale = isActive ? Vector3.zero : Vector3.one;
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

        /// <summary>
        /// Same as SetActive, but disables whole element, not just body.
        /// </summary>
        /// <param name="isCollapsed"></param>
        public async Task SetCollapsed(bool isCollapsed)
        {
            TaskCompletionSource<bool> tcs = new();
            
            if (isCollapsed)
            {
                await SetActive(false);
                _isCollapsed = true;
                gameObject.SetActive(false);
                tcs.SetResult(true);
            }
            else
            {
                gameObject.SetActive(true);
                await SetActive(true);
                _isCollapsed = false;
                tcs.SetResult(true);
            }
            
            await tcs.Task;
        }

        public bool IsActive => body.activeSelf;
    }
}