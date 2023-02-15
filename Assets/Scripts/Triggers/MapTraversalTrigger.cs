using Scripts.EventsManagement;

namespace Scripts.Triggers
{
    public class MapTraversalTrigger : Trigger, IEventTrigger
    {
        public string targetMapName;
        public string targetMapEntryPoint;
        public float delay;

        internal override void OnTriggerActivated(Enums.ETriggerActivatedDetail activatedDetail = Enums.ETriggerActivatedDetail.None)
        {
            EventsManager.TriggerOnMapTraversalTriggered(targetMapName, targetMapEntryPoint, delay);
        }
    }
}