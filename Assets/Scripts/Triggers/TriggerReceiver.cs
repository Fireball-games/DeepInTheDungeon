﻿using System;
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
        public int startMovement;
        public string identification;
        [NonSerialized] public string PrefabGuid;
        
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
            if (AtRest && source.subscribers.Contains(PrefabGuid))
            {
                TriggerNext();
            }
        }

        protected abstract void TriggerNext();

        protected void SetResting() => AtRest = true;

        protected void SetBusy() => AtRest = false;
        public abstract void SetMovementStep();
    }
}