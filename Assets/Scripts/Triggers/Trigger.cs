using System;
using System.Collections.Generic;
using Scripts.Building.Walls;
using Scripts.EventsManagement;
using Scripts.Helpers.Extensions;
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
            if (atRest && IsPositionValid())
            {
                OnTriggerActivated();
            }
        }

        protected abstract void OnTriggerActivated();
        
        protected void TriggerNext()
        {
            EventsManager.TriggerOnTriggerNext(subscribers);
        }

        protected void SetResting(bool isAtRest) => atRest = isAtRest;

        private bool IsPositionValid()
        {
            Vector3 position = transform.position;
            
            bool sameTileValidation = !mustBeOnSameTile || GameManager.Instance.Player.transform.position.ToVector3Int() == position.ToVector3Int();
            
            return sameTileValidation && (position - Player.transform.position).sqrMagnitude < MaxDistanceFromPlayer;
        }
    }
}