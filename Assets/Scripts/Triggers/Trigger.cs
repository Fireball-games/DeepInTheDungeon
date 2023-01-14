using System;
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
        /// <summary>
        /// How many times can be trigger triggered.
        /// </summary>
        public int count;
        public bool mustBeOnSameTile = true;
        public float actionDuration = 0.3f;
        public bool atRest = true;
        public List<TriggerReceiver> presetSubscribers;
        public List<string> subscribers;

        protected static PlayerController Player => GameManager.Instance.Player;
        protected Transform ActivePart;
        protected int StartMovement;
        protected int CurrentMovement;
        
        protected virtual void Awake()
        {
            subscribers = new List<string>();
            ActivePart = transform.Find("ActivePart");
        }

        private void Start()
        {
            foreach (TriggerReceiver triggerReceiver in presetSubscribers)
            {
                if (!subscribers.Contains(triggerReceiver.Guid))
                {
                    subscribers.Add(triggerReceiver.Guid);
                }
            }

            count = triggerType switch
            {
                ETriggerType.OneOff => 1,
                ETriggerType.Repeat => int.MaxValue,
                ETriggerType.Multiple => count,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        protected abstract void OnTriggerActivated();
        
        protected void TriggerNext(bool setAtRest = false)
        {
            if (count <= 0) return;
            
            atRest = setAtRest;
            EventsManager.TriggerOnTriggerNext(this);
        }

        protected void SetResting(bool isAtRest) => atRest = isAtRest;

        protected bool IsPositionValid()
        {
            Vector3 position = transform.position;
            
            bool sameTileValidation = !mustBeOnSameTile || Vector3.Distance(GameManager.Instance.PlayerPosition,position) <= 1f;
            
            return sameTileValidation && (position - Player.transform.position).sqrMagnitude <= MaxDistanceFromPlayer;
        }

        public override void InitializeFromPool()
        {
            base.InitializeFromPool();
            
            subscribers.Clear();
        }
    }
}