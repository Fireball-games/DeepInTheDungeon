using System.Collections.Generic;
using UnityEngine;
using static Scripts.Enums;

namespace Scripts.Triggers
{
    public abstract class VolumeTrigger : Trigger
    {
        protected bool IsTriggerQueued;
        protected List<GameObject> EnteredObjects;

        private void OnTriggerEnter(Collider other)
        {
            if (!IsTriggerQueued) IsTriggerQueued = true;
            EnteredObjects.Add(other.gameObject);
            OnTriggerActivated(ETriggerActivatedDetail.SwitchedOn);
        }

        private void OnTriggerExit(Collider other)
        {
            IsTriggerQueued = false;
            EnteredObjects.Remove(other.gameObject);
            OnTriggerActivated(ETriggerActivatedDetail.SwitchedOff);
        }
    }
}