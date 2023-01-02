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
        [SerializeField] private Transform activePart;

        [NonSerialized] public string Guid;

        protected bool AtRest;
        
        private void OnEnable()
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
    }
}