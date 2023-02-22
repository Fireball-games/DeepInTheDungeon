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
        public int CurrentPosition { get; protected set; }
        public string identification;
        
        private bool _atRest = true;

        protected virtual void Awake()
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
            if (_atRest && source.subscribers.Contains(Guid))
            {
                TriggerNext();
            }
        }

        protected abstract void TriggerNext();

        protected void SetResting() => _atRest = true;

        protected void SetBusy() => _atRest = false;
        
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
            
            positionsTrigger.SetCurrentPosition(saveData.currentPosition);
        }
    }
}