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
        public ETriggerType triggerType;
        public bool mustBeOnSameTile = true;
        public float maxDistanceFromPlayer = 0.7f;
        protected Transform ActivePart;

        protected List<string> Subscribers;
        protected static PlayerController Player => GameManager.Instance.Player;
        protected int CurrentMovement;

        protected virtual void Awake()
        {
            Subscribers = new List<string>();
            ActivePart = transform.Find("ActivePart");
        }
        
        private void OnMouseUp()
        {
            if ((transform.position - Player.transform.position).sqrMagnitude < maxDistanceFromPlayer)
            {
                TriggerNext();
            }
        }

        protected abstract void OnTriggerNext();
        
        protected void TriggerNext()
        {
            OnTriggerNext();
            EventsManager.TriggerOnTriggerNext(Subscribers);
        }
    }
}