using System.Collections.Generic;
using Scripts.Helpers;
using Scripts.Helpers.Extensions;
using NotImplementedException = System.NotImplementedException;

namespace Scripts.Triggers
{
    public class StateScriptTriggerReceiver : TriggerReceiver, IPositionsTrigger
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

        public List<DoTweenMoveStep> GetSteps() => new(2);

        public int GetCurrentPosition() => _currentState ? 0 : 1;

        public void SetCurrentPosition(int newPosition)
        {
            _target.SetState(newPosition);
        }
    }
}