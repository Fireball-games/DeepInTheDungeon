using System.Threading.Tasks;
using DG.Tweening;
using Scripts.System.MonoBases;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Scripts.ScenesManagement
{
    public class ScreenFader : Singleton<ScreenFader>
    {
        private static Image _faderImage;
        private static GameObject _gameObject;
        private static TaskCompletionSource<bool> _taskCompletionSource;

        public static UnityEvent OnFadeFinished { get; } = new();

        protected override void Awake()
        {
            base.Awake();
            
            _faderImage = GetComponentInChildren<Image>();
            _gameObject = gameObject;
        }
        
        public static async Task FadeIn(float duration)
        {
            _gameObject.SetActive(true);
            _faderImage.color = new Color(0, 0, 0, 0);

            _faderImage.DOFade(1, duration).OnComplete(() => 
            {
                _taskCompletionSource.SetResult(true);
            }).Play();

            TaskCompletionSource<bool> tcs = new();
            _taskCompletionSource = tcs;

            await tcs.Task;
        }
        
        public static void FadeOut(float duration, UnityAction onFadeOutFinished = null)
        {
            _faderImage.color = new Color(0, 0, 0, 1);
            _faderImage.DOColor(new Color(0,0,0,0), duration).Play().OnComplete(() =>
            {
                _gameObject.SetActive(false);
                onFadeOutFinished?.Invoke();
            });
        }
        
        private void OnFadeFinished_internal()
        {
            OnFadeFinished.Invoke();
        }
    }
}