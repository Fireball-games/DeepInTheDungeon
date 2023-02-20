using Scripts.Building.PrefabsSpawning;
using Scripts.EventsManagement;
using Scripts.System.Saving;
using UnityEngine;
using Logger = Scripts.Helpers.Logger;

namespace Scripts.Triggers
{
    [RequireComponent(typeof(PrefabBase))]
    public abstract class TriggerReceiver : MonoBehaviour, ISavable
    {
        [SerializeField] protected Transform activePart;
        public string Guid { get; set; }
        public int startPosition;
        public string identification;
        
        protected int CurrentPosition;

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
        
        public object CaptureState() =>
            new TriggerSaveData
            {
                count = 0,
                currentPosition = CurrentPosition,
            };

        public void RestoreState(object state)
        {
            if (state is not TriggerSaveData saveData)
            {
                Logger.LogError("Invalid save data.", logObject: this);
                return;
            }

            if (this is not IPositionsTrigger positionsTrigger) return;
            
            positionsTrigger.SetStartPosition(saveData.currentPosition);
            positionsTrigger.SetPosition();
        }
    }
}