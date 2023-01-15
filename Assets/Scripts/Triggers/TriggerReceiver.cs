using Scripts.Building.Walls;
using Scripts.EventsManagement;
using UnityEngine;

namespace Scripts.Triggers
{
    [RequireComponent(typeof(PrefabBase))]
    public abstract class TriggerReceiver : MonoBehaviour
    {
        [SerializeField] protected Transform activePart;
        public string Guid;  
        public int startPosition;
        public string identification;
        
        protected int CurrentMovement;

        protected bool AtRest = true;

        private void Awake()
        {
            Guid = global::System.Guid.NewGuid().ToString();
        }

        protected virtual void OnEnable()
        {
            EventsManager.OnTriggerNext += OnTriggerNext;
        }

        private void OnDisable()
        {
            EventsManager.OnTriggerNext -= OnTriggerNext;
        }

        private void OnTriggerNext(Trigger source)
        {
            if (AtRest && source.subscribers.Contains(Guid))
            {
                TriggerNext();
            }
        }

        protected abstract void TriggerNext();

        protected void SetResting() => AtRest = true;

        protected void SetBusy() => AtRest = false;
        public abstract void SetPosition();
    }
}