using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using static Scripts.Enums;

namespace Scripts.Triggers
{
    public class DoTweenTrigger : MouseClickTrigger
    {
        public ETriggerMoveType moveType;
        public EActiveProperty activeProperty;
        public List<DoTweenMoveStep> steps;

        private List<Tween> _movementStore;

        protected override void Awake()
        {
            base.Awake();
            _movementStore = new List<Tween>();
        }
        
        private void OnEnable()
        {
            _movementStore?.Clear();

            for (int index = 0; index < steps.Count; index++)
            {
                Tween newTween = BuildTween(steps[index]);

                if (index == steps.Count - 1)
                {
                    newTween.OnComplete(() => SetResting());
                }
                else
                {
                    newTween.OnPlay(SetBusy);
                }

                _movementStore?.Add(newTween);
            }
        }
        
        public void SetMovementStep()
        {
            if (activeProperty is EActiveProperty.Position)
            {
                ActivePart.localPosition = steps[StartMovement].target;
            }
            else
            {
                ActivePart.localRotation = Quaternion.Euler(steps[StartMovement].target);
            }

            CurrentMovement = StartMovement == steps.Count - 1 ? 0 : StartMovement + 1;
        }

        protected override void OnTriggerActivated()
        {
            switch (moveType)
            {
                case ETriggerMoveType.ThereAndBack:
                    CurrentMovement = 0;
                    _movementStore[0].OnComplete(() => RunBackTween(true))
                        .Restart();
                    break;
                case ETriggerMoveType.Switch:
                    if (CurrentMovement == 0)
                    {
                        _movementStore[0].OnComplete(() => SetResting(true)).Restart();
                        CurrentMovement = 1;
                    }
                    else
                    {
                        _movementStore[1].OnComplete(() => SetResting(true)).Restart();
                        CurrentMovement = 0;
                    }

                    break;
                case ETriggerMoveType.None:
                    break;
                case ETriggerMoveType.Periodic:
                    break;
                case ETriggerMoveType.Multiple:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void RunBackTween(bool triggerNextOnStart = false)
        {
            if (triggerNextOnStart)
            {
                TriggerNext();
            }

            _movementStore[^1].Restart();
        }
        
        private Tween BuildTween(DoTweenMoveStep step)
            => BuildTween(step.target, step.duration, step.movementEase);

        private Tween BuildTween(Vector3 target, float duration, Ease ease = Ease.Linear)
        {
            Tween result = activeProperty is EActiveProperty.Position
                ? ActivePart.DOLocalMove(target, duration)
                : ActivePart.DOLocalRotate(target, duration);

            result.SetEase(ease).SetAutoKill(false);

            return result;
        }
    }
}