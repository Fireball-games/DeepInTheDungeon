using Scripts.EventsManagement;

namespace Scripts.Triggers
{
    public class ExitPointTrigger : Trigger, IEventTrigger
    {
        internal override void OnTriggerActivated(Enums.ETriggerActivatedDetail activatedDetail = Enums.ETriggerActivatedDetail.None)
        {
            if (activatedDetail is Enums.ETriggerActivatedDetail.SwitchedOn)
            {
                EventsManager.TriggerOnMapTraversalTriggered(Guid);
            }
        }
    }
}