using System.Collections.Generic;
using Scripts.System.MonoBases;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.UI
{
    public class Modal : MonoBehaviour
    {
        [SerializeField] private Button body;

        private static int openCount;
        private static GameObject _body;

        private static Stack<OpenQueueItem> _openedQueue;

        private void Awake()
        {
            body.onClick.AddListener(OnModalClicked);
            _body = body.gameObject;
            _openedQueue = new Stack<OpenQueueItem>();
        }
        
        public static void Show(DialogBase subscriber, bool closeOnclick = true)
        {
            _body.gameObject.SetActive(true);

            _openedQueue.Push(new OpenQueueItem(subscriber, closeOnclick));
        }

        public static void Hide()
        {
            _openedQueue.Pop();
            
            if (_openedQueue.Count > 0) return;

            _body.SetActive(false);
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