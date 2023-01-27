using System.Collections.Generic;
using Scripts.EventsManagement;
using Scripts.System.MonoBases;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.UI
{
    public class Modal : MonoBehaviour
    {
        [SerializeField] private Button body;

        private int openCount;

        private static Stack<DialogBase> _openedQueue;

        private void Awake()
        {
            body.onClick.AddListener(OnModalClicked);
            _openedQueue = new Stack<DialogBase>();
        }

        private void OnEnable()
        {
            EventsManager.OnModalShowRequested += Activate;
            EventsManager.OnModalHideRequested += Deactivate;
        }

        private void OnDisable()
        {
            EventsManager.OnModalShowRequested -= Activate;
            EventsManager.OnModalHideRequested -= Deactivate;
        }

        public static void Hide() => EventsManager.TriggerOnModalHideRequested();
        public static void Show() => EventsManager.TriggerOnModalShowRequested();
        
        public static void SubscribeToClick(DialogBase subscriber) => _openedQueue.Push(subscriber);

        private void OnModalClicked()
        {
            if (_openedQueue.Count == 0) return;
            
            _openedQueue.Pop().CloseDialog();   
        }

        private void Activate()
        {
            if(!body.IsActive()) openCount = 0;
            openCount += 1;
            SetActive(true);
        }

        private void Deactivate()
        {
            if (body.IsActive())
            {
                openCount -= 1;
            }

            if (openCount > 0) return;
            
            SetActive(false);
        }

        private void SetActive(bool isActive) => body.gameObject.SetActive(isActive);
    }
}
