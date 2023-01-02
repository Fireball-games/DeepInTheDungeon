using System;
using System.Collections.Generic;
using Scripts.Building.Walls;
using Scripts.EventsManagement;
using Scripts.Player;
using Scripts.System;
using UnityEngine;
using UnityEngine.Serialization;
using static Scripts.Enums;

namespace Scripts.Triggers
{
    public abstract class Trigger : PrefabBase
    {
        private const float MaxDistanceFromPlayer = 0.7f;
        
        public ETriggerType triggerType;
        public bool mustBeOnSameTile = true;
        public float actionDuration = 0.3f;
        public bool atRest = true;
        public List<PrefabBase> presetSubscribers;

        protected static PlayerController Player => GameManager.Instance.Player;
        protected Transform ActivePart;
        protected int StartMovement;
        protected int CurrentMovement;
        
        private List<string> subscribers;

        protected virtual void Awake()
        {
            subscribers = new List<string>();
            ActivePart = transform.Find("ActivePart");
        }

        private void Start()
        {
            foreach (PrefabBase prefab in presetSubscribers)
            {
                if (!subscribers.Contains(prefab.GUID))
                {
                    subscribers.Add(prefab.GUID);
                }
            }
        }

        private void OnMouseUp()
        {
            if (atRest && (transform.position - Player.transform.position).sqrMagnitude < MaxDistanceFromPlayer)
            {
                OnTriggerActivated();
            }
        }

        protected abstract void OnTriggerNext();

        protected abstract void OnTriggerActivated();
        
        protected void TriggerNext()
        {
            EventsManager.TriggerOnTriggerNext(subscribers);
        }

        protected void SetResting(bool isAtRest) => atRest = isAtRest;
    }
}