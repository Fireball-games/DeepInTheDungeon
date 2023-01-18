using System.Collections.Generic;
using UnityEngine;
using static Scripts.Enums;

namespace Scripts.Triggers
{
    public abstract class VolumeTrigger : Trigger
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
            OnTriggerActivated(ETriggerActivatedDetail.SwitchedOn);
        }

        private void OnTriggerExit(Collider other)
        {
            if ((allowedLayers.value & (1 << other.gameObject.layer)) == 0) return;
            
            IsTriggerQueued = false;
            EnteredObjects.Remove(other.gameObject);
            OnTriggerActivated(ETriggerActivatedDetail.SwitchedOff);
        }
    }
}