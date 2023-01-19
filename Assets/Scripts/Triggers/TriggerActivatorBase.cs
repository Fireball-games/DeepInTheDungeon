using UnityEngine;
using Logger = Scripts.Helpers.Logger;

namespace Scripts.Triggers
{
    public abstract class TriggerActivatorBase : MonoBehaviour
    {
        protected Trigger TargetTrigger;

        protected virtual void Awake()
        {
            TargetTrigger = GetComponent<Trigger>();

            if (!TargetTrigger)
            {
                Logger.LogError("Detected TriggerActivator without target trigger.", logObject: this);
            }
        }
    }
}