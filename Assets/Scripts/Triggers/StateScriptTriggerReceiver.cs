﻿using System.Collections.Generic;
using Scripts.Helpers;
using Scripts.Helpers.Extensions;
using Scripts.System.Pooling;

namespace Scripts.Triggers
{
    public class StateScriptTriggerReceiver : TriggerReceiver, IPositionsTrigger, IPoolInitializable
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
                CurrentPosition = _currentState ? 0 : 1;
                _currentState = !_currentState;
                _target.RunState(_currentState);
            }
        }

        public List<DoTweenMoveStep> GetSteps() => new(2);

        public int GetCurrentPosition() => _currentState ? 1 : 0;

        public void SetCurrentPosition(int newPosition)
        {
            _currentState = newPosition == 1;
            _target.SetState(newPosition);
        }

        public void InitializeFromPool()
        {
            SetCurrentPosition(0);
        }
    }
}