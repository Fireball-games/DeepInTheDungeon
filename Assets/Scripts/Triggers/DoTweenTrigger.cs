using DG.Tweening;
using UnityEngine;
using static Scripts.Enums;

namespace Scripts.Triggers
{
    public class DoTweenTrigger : Trigger
    {
        public ETriggerMoveType moveType;
        public EActiveProperty activeProperty;
        public Vector3 movementVector;

        private Tween _there;
        private Tween _back;

        protected override void Awake()
        {
            base.Awake();

            _there = activeProperty is EActiveProperty.Position
                ? ActivePart.DOLocalMove(movementVector, actionDuration).SetAutoKill(false)
                : ActivePart.DOLocalRotate(movementVector, actionDuration).SetAutoKill(false);
            _there.OnPlay(() => SetResting(false));

            _back = activeProperty is EActiveProperty.Position
                ? ActivePart.DOLocalMove(Vector3.zero, actionDuration).SetAutoKill(false)
                : ActivePart.DOLocalRotate(Vector3.zero, actionDuration).SetAutoKill(false);
            _back.OnComplete(() => SetResting(true));
        }

        protected override void OnTriggerActivated()
        {
            switch (moveType)
            {
                case ETriggerMoveType.ThereAndBack:
                    CurrentMovement = 0;
                    _there.OnComplete(() => RunBackTween(true)).Restart();
                    break;
                case ETriggerMoveType.Switch:
                    if (CurrentMovement == 0)
                    {
                        CurrentMovement = 1;
                        _there.OnComplete(TriggerNext).Restart();
                    }
                    else
                    {
                        CurrentMovement = 0;
                        _back.OnComplete(TriggerNext).Restart();
                    }
                    break;
            }
        }

        private void RunBackTween(bool triggerNextOnStart = false)
        {
            if (triggerNextOnStart)
            {
                TriggerNext();
            }
            
            _back.Restart();
        }
    }
}