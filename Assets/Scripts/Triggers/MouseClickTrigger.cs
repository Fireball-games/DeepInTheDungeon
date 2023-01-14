
namespace Scripts.Triggers
{
    public abstract class MouseClickTrigger : Trigger
    {
        private void OnMouseUp()
        {
            if (atRest && IsPositionValid() && count > 0)
            {
                OnTriggerActivated();
            }
        }
    }
}