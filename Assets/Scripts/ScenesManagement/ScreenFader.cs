using System.Threading.Tasks;
using DG.Tweening;
using Scripts.Helpers;
using Scripts.System.MonoBases;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Scripts.ScenesManagement
{
    public class ScreenFader : Singleton<ScreenFader>
    {
        private static Image _faderImage;
        private static Image _splashImage;
        private static GameObject _gameObject;
        private static TaskCompletionSource<bool> _taskCompletionSource;
        
        private static Tween _splashTweenIn;
        private static Tween _splashTweenOut;

        public static UnityEvent OnFadeFinished { get; } = new();

        protected override void Awake()
        {
            base.Awake();
            
            _faderImage = transform.Find("FaderImage").GetComponent<Image>();
            _splashImage = _faderImage.transform.Find("SplashImage").GetComponent<Image>();
            _splashTweenIn = _splashImage.DOFade(1, 1).SetAutoKill(false).SetEase(Ease.InOutSine);
            _splashTweenOut = _splashImage.DOFade(0, 1).SetAutoKill(false).SetEase(Ease.InOutSine);
            _gameObject = gameObject;
            
        }
        
        public static async Task FadeIn(float duration)
        {
            _gameObject.SetActive(true);
            _faderImage.color = Colors.FullTransparentBlack;

            _faderImage.DOFade(1, duration).OnComplete(() => 
            {
                _taskCompletionSource.SetResult(true);
            }).Play();
            
            if (_splashTweenOut.IsPlaying())
            {
                _splashTweenOut.Pause();
            }
            _splashTweenIn = _splashImage.DOFade(1, duration/2)
                .SetAutoKill(false)
                .SetEase(Ease.InOutSine)
                .SetDelay(duration/2)
                .Play();

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
            
            if (_splashTweenIn.IsPlaying())
            {
                _splashTweenIn.Pause();
            }
            _splashTweenOut = _splashImage.DOFade(0, duration/2)
                .SetAutoKill(false)
                .SetEase(Ease.InOutSine)
                .Play();
        }
    }
}