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

        protected override void Awake()
        {
            base.Awake();

            if (moveType is ETriggerMoveType.ThereAndBack)
            {
                if (activeProperty is EActiveProperty.Position)
                {
                    _thereAndBackSequenceForPosition = DOTween.Sequence()
                        .Append(ActivePart.DOLocalMove(movementVector, 0.3f))
                        .Append(ActivePart.DOLocalMove(Vector3.zero, 0.3f));
                }
            }
        }

        protected override void OnTriggerNext()
        {
            if (moveType == ETriggerMoveType.ThereAndBack)
            {
                switch (activeProperty)
                {
                    case EActiveProperty.None:
                        break;
                    case EActiveProperty.Position:
                        DOTween.Sequence()
                            .Append(ActivePart.DOLocalMove(movementVector, 0.3f))
                            .Append(ActivePart.DOLocalMove(Vector3.zero, 0.3f)).Play();
                        break;
                    case EActiveProperty.Rotation:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}