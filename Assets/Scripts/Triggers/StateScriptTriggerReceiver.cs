using Scripts.Helpers;
using Scripts.Helpers.Extensions;

namespace Scripts.Triggers
{
    public class StateScriptTriggerReceiver : TriggerReceiver
    {
        private bool _currentState;
        private StateTriggerTarget _target;

        protected override void Awake()
        {
            base.Awake();
            _target = GetComponentInChildren<StateTriggerTarget>();
            _target.isAtRest = true;
            
            if (_target == null)
            {
                Logger.LogWarning($"{nameof(StateScriptTriggerReceiver).WrapInColor(Colors.LightBlue)} requires a {nameof(StateTriggerTarget).WrapInColor(Colors.Warning)} component");
            }
        }

        protected override void TriggerNext()
        {
            if (_target.isAtRest)
            {
                _currentState = !_currentState;
                _target.RunState(_currentState);
            }
        }
    }
}