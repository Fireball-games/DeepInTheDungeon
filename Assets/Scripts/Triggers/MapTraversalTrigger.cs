using Scripts.EventsManagement;

namespace Scripts.Triggers
{
    public class MapTraversalTrigger : Trigger, IEventTrigger
    {
        internal override void OnTriggerActivated(Enums.ETriggerActivatedDetail activatedDetail = Enums.ETriggerActivatedDetail.None)
        {
            EventsManager.TriggerOnMapTraversalTriggered(Guid);
        }
    }
}