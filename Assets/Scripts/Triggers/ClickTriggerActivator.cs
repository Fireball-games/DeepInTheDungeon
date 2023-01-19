namespace Scripts.Triggers
{
    public class ClickTriggerActivator : TriggerActivatorBase
    {
        public void OnMouseUp()
        {
            if (TargetTrigger.atRest && TargetTrigger.IsPositionValid() && TargetTrigger.count > 0)
            {
                TargetTrigger.OnTriggerActivated();
            }
        }
    }
}