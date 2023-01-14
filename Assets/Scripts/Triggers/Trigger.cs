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
        [Header("Behaviour settings")]
        public ETriggerType triggerType;
        /// <summary>
        /// How many times can be trigger triggered.
        /// </summary>
        public int count;
        public bool mustBeOnSameTile = true;
        public bool atRest = true;
        public List<TriggerReceiver> presetSubscribers;
        public List<string> subscribers;
        [Header("Movement settings")]
        public int startMovement;

        protected static PlayerController Player => GameManager.Instance.Player;
        protected Transform ActivePart;
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

            if (triggerType != ETriggerType.Repeat) count -= 1;
            
            atRest = setAtRest;
            EventsManager.TriggerOnTriggerNext(this);
        }

        protected void SetResting(bool triggerNextOnStart = false)
        {
            if (triggerNextOnStart)
            {
                TriggerNext();
            }
            
            atRest = true;
        }

        protected void SetBusy() => atRest = false;

        protected bool IsPositionValid()
        {
            Vector3 position = transform.position;
            
            bool sameTileValidation = !mustBeOnSameTile || Vector3.Distance(GameManager.Instance.PlayerPosition,position) <= 1f;
            
            return sameTileValidation && (position - Player.transform.position).sqrMagnitude <= MaxDistanceFromPlayer;
        }
        
        public abstract void SetMovementStep();

        public override void InitializeFromPool()
        {
            base.InitializeFromPool();
            
            subscribers.Clear();
        }
    }
}