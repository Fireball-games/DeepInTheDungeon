using System;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Scripts.ScenesManagement
{
    public class ScreenFader : MonoBehaviour
    {
        private static Image _faderImage;
        private static TaskCompletionSource<bool> _taskCompletionSource;

        public static UnityEvent OnFadeFinished { get; } = new();

        private void Awake()
        {
            _faderImage = GetComponentInChildren<Image>();
        }
        
        public static async Task FadeIn(float duration)
        {
            _faderImage.color = new Color(0, 0, 0, 0);

            _faderImage.DOFade(1, duration).OnComplete(() => 
            {
                _taskCompletionSource.SetResult(true);
            }).Play();

            TaskCompletionSource<bool> tcs = new();
            _taskCompletionSource = tcs;

            await tcs.Task;
        }
        
        public static void FadeOut(float duration)
        {
            _faderImage.color = new Color(0, 0, 0, 1);
            _faderImage.DOFade(0, duration).Play();
        }
        
        private void OnFadeFinished_internal()
        {
            OnFadeFinished.Invoke();
        }
    }
}