using System;
using System.Collections.Generic;
using Scripts.Building.Walls;
using Scripts.EventsManagement;
using UnityEngine;

namespace Scripts.Triggers
{
    [RequireComponent(typeof(PrefabBase))]
    public abstract class TriggerReceiver : MonoBehaviour
    {
        [SerializeField] protected Transform activePart;
        [NonSerialized] public string Guid;
        public int startMovement;
        
        protected int CurrentMovement;

        protected bool AtRest = true;

        protected virtual void OnEnable()
        {
            EventsManager.OnTriggerNext += OnTriggerNext;
        }

        private void OnDisable()
        {
            EventsManager.OnTriggerNext -= OnTriggerNext;
        }

        private void OnTriggerNext(List<string> triggeredGuids)
        {
            if (AtRest && triggeredGuids.Contains(Guid))
            {
                TriggerNext();
            }
        }

        protected abstract void TriggerNext();

        protected void SetResting() => AtRest = true;

        protected void SetBusy() => AtRest = false;
    }
}