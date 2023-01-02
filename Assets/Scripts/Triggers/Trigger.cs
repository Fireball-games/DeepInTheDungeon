using System.Collections.Generic;
using Scripts.Building.Walls;
using Scripts.EventsManagement;
using Scripts.Player;
using Scripts.System;
using UnityEngine;
using static Scripts.Enums;

namespace Scripts.Triggers
{
    public abstract class Trigger : PrefabBase
    {
        private const float MaxDistanceFromPlayer = 0.7f;
        
        public ETriggerType triggerType;
        public bool mustBeOnSameTile = true;
        public float actionDuration = 0.3f;
        public bool AtRest;

        protected Transform ActivePart;
        protected List<string> Subscribers;
        protected static PlayerController Player => GameManager.Instance.Player;
        protected int StartMovement;
        protected int CurrentMovement;

        protected virtual void Awake()
        {
            Subscribers = new List<string>();
            ActivePart = transform.Find("ActivePart");
        }
        
        private void OnMouseUp()
        {
            if (AtRest && (transform.position - Player.transform.position).sqrMagnitude < MaxDistanceFromPlayer)
            {
                OnTriggerActivated();
            }
        }

        protected abstract void OnTriggerNext();

        protected abstract void OnTriggerActivated();
        
        protected void TriggerNext()
        {
            EventsManager.TriggerOnTriggerNext(Subscribers);
        }

        protected void SetResting(bool isAtRest) => AtRest = isAtRest;
    }
}