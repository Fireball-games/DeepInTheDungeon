﻿using System;
using System.Collections.Generic;
using Scripts.Building.PrefabsSpawning;
using Scripts.EventsManagement;
using Scripts.Player;
using Scripts.System;
using Scripts.System.Saving;
using UnityEngine;
using static Scripts.Enums;
using Logger = Scripts.Helpers.Logger;

namespace Scripts.Triggers
{
    public abstract class Trigger : PrefabBase, ISavable
    {
        private const float MaxDistanceFromPlayer = 0.7f;
        [Header("Behaviour settings")] public ETriggerType triggerType;

        /// <summary>
        /// How many times can be trigger triggered.
        /// </summary>
        public int count;

        public bool mustBeOnSameTile = true;
        public bool atRest = true;
        public List<TriggerReceiver> presetSubscribers;
        public List<string> subscribers;

        protected Transform ActivePart;
        protected int CurrentPosition;

        private static PlayerController Player => GameManager.Instance.Player;

        protected virtual void Awake()
        {
            subscribers = new List<string>();
            ActivePart = transform.Find("ActivePart");

            if (!GetComponent<TriggerActivatorBase>())
            {
                Logger.LogError("Detected trigger without TriggerActivator.", logObject: this);
            }
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

        internal abstract void OnTriggerActivated(ETriggerActivatedDetail activatedDetail = ETriggerActivatedDetail.None);

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

        internal bool IsPositionValid()
        {
            Vector3 position = transform.position;

            bool sameTileValidation = !mustBeOnSameTile || Vector3.Distance(GameManager.Instance.PlayerPosition, position) <= 1f;

            return sameTileValidation && (position - Player.transform.position).sqrMagnitude <= MaxDistanceFromPlayer;
        }

        public override void InitializeFromPool()
        {
            base.InitializeFromPool();

            subscribers.Clear();
        }

        public object CaptureState() =>
            new TriggerSaveData
            {
                count = count,
                currentPosition = CurrentPosition,
            };

        public void RestoreState(object state)
        {
            if (state is not TriggerSaveData saveData)
            {
                Logger.LogError("Invalid save data.", logObject: this);
                return;
            }
             
            count = saveData.count;

            if (this is not IPositionsTrigger positionsTrigger) return;
            
            positionsTrigger.SetStartPosition(saveData.currentPosition);
            positionsTrigger.SetPosition();
        }
    }

    [Serializable]
    public class TriggerSaveData
    {
        public int count;
        public int currentPosition;
    }
}