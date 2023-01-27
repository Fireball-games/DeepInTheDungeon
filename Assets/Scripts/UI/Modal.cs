using Scripts.EventsManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.UI
{
    public class Modal : MonoBehaviour
    {
        [SerializeField] private Button body;

        private int openCount;

        private void Awake()
        {
            body.onClick.AddListener(OnModalClicked);
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

        private void OnModalClicked() => EventsManager.TriggerOnModalClicked();

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
