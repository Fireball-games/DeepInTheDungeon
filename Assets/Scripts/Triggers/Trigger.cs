using System.Collections.Generic;
using Scripts.Building.Walls;
using Scripts.EventsManagement;
using static Scripts.Enums;

namespace Scripts.Triggers
{
    public abstract class Trigger : PrefabBase
    {
        public ETriggerType triggerType;

        protected List<string> Subscribers;

        protected Trigger() : base()
        {
            Subscribers = new List<string>();
        }

        protected abstract void OnTriggerNext();
        
        protected void TriggerNext()
        {
            OnTriggerNext();
            EventsManager.TriggerOnTriggerNext(Subscribers);
        }
    }
}