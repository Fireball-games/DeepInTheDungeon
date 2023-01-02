using System;
using System.Collections.Generic;
using Scripts.Building.Walls;
using Scripts.EventsManagement;
using UnityEngine;

namespace Scripts.Triggers
{
    [RequireComponent(typeof(PrefabBase))]
    public class TriggerReceiver : MonoBehaviour
    {
        [SerializeField] private Transform activePart;
        
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
            
        }
    }
}