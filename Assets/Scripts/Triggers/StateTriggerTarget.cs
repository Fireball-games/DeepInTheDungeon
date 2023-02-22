using System.Threading.Tasks;
using UnityEngine;

namespace Scripts.Triggers
{
    public abstract class StateTriggerTarget : MonoBehaviour
    {
        public bool isAtRest = true;

        protected abstract Task RunOnState();
        protected abstract Task RunOffState();
        
        public async void RunState(bool onState)
        {
            isAtRest = false;
            if (onState)
            {
                await RunOnState();
            }
            else
            {
                await RunOffState();
            }
            isAtRest = true;
        }
    }
}