using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Triggers
{
    public class VolumeTriggerActivator : TriggerActivatorBase
    {
        protected bool IsTriggerQueued;
        protected List<GameObject> EnteredObjects;
        public LayerMask allowedLayers;

        protected override void Awake()
        {
            base.Awake();
            
            EnteredObjects = new List<GameObject>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if ((allowedLayers.value & (1 << other.gameObject.layer)) == 0) return;
            
            if (!IsTriggerQueued) IsTriggerQueued = true;
            EnteredObjects.Add(other.gameObject);
            TargetTrigger.OnTriggerActivated(Enums.ETriggerActivatedDetail.SwitchedOn);
        }

        private void OnTriggerExit(Collider other)
        {
            if ((allowedLayers.value & (1 << other.gameObject.layer)) == 0) return;
            
            IsTriggerQueued = false;
            EnteredObjects.Remove(other.gameObject);
            TargetTrigger.OnTriggerActivated(Enums.ETriggerActivatedDetail.SwitchedOff);
        }
    }
}