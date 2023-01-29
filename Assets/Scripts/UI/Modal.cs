using System.Collections.Generic;
using DG.Tweening;
using Scripts.System.MonoBases;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.UI
{
    public class Modal : MonoBehaviour
    {
        private static Button _button;
        private static Image _background;

        private static GameObject _body;
        private static int _openCount;
        private const float BackgroundMaxScale = 23f;
        private const float AnimationDuration = 0.3f;

        private static Stack<OpenQueueItem> _openedQueue;

        private void Awake()
        {
            _body = transform.Find("Body").gameObject;
            _body.SetActive(false);
            
            _button = _body.transform.Find("Button").GetComponent<Button>();
            _button.onClick.AddListener(OnModalClicked);
            
            _background = _body.transform.Find("Background").GetComponent<Image>();
            
            _openedQueue = new Stack<OpenQueueItem>();
        }
        
        public static void Show(DialogBase subscriber, bool closeOnclick = true)
        {
            _body.SetActive(true);

            _openedQueue.Push(new OpenQueueItem(subscriber, closeOnclick));

            if (!_background || _background.transform.localScale.x != 0) return;
            
            _background.transform.localScale = Vector3.zero;
            _background.transform.DOScale(BackgroundMaxScale, AnimationDuration).Play();
        }

        public static void Hide()
        {
            _openedQueue.Pop();
            
            if (!_background || _openedQueue.Count > 0) return;
            
            _background.transform.DOScale(0, AnimationDuration)
                .OnComplete(() => _body.SetActive(false))
                .Play();
        }

        private void OnModalClicked()
        {
            OpenQueueItem item = _openedQueue.Peek();

            if (!item.CloseModalOnClick) return;
            
            item.Subscriber.CloseDialog();
        }
    }

    public class OpenQueueItem
    {
        public readonly DialogBase Subscriber;
        public readonly bool CloseModalOnClick;

        public OpenQueueItem(DialogBase subscriber, bool closeModalOnClick)
        {
            Subscriber = subscriber;
            CloseModalOnClick = closeModalOnClick;
        }
    }
}