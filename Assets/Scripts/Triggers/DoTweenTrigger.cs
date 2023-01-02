using System;
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

        private Sequence _thereAndBackSequenceForPosition;
        private Tween _there;
        private Tween _back;
        private Tween _thereOneOff;
        private Tween _backForPosition;

        protected override void Awake()
        {
            base.Awake();

            _there = ActivePart.DOLocalMove(movementVector, 0.3f).SetAutoKill(false);
            _there.OnPlay(() => SetResting(false));

            _back = ActivePart.DOLocalMove(Vector3.zero, 0.3f).SetAutoKill(false);
            _back.OnComplete(() => SetResting(true));
        }

        protected override void OnTriggerNext()
        {
            
        }

        protected override void OnTriggerActivated()
        {
            if (moveType == ETriggerMoveType.ThereAndBack)
            {
                switch (activeProperty)
                {
                    case EActiveProperty.None:
                        break;
                    case EActiveProperty.Position:
                        _there.OnComplete(() => RunBackTween(true)).Restart();
                        break;
                    case EActiveProperty.Rotation:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            // TODO: other movement types
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