using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using static Scripts.Enums;

namespace Scripts.Triggers
{
    public class DoTweenTriggerReceiver : TriggerReceiver, IPositionsTrigger
    {
        public ETriggerMoveType moveType;
        public EActiveProperty activeProperty;
        public List<DoTweenMoveStep> steps;

        private List<Tween> _movementStore;

        private void Awake()
        {
            _movementStore = new List<Tween>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            
            _movementStore?.Clear();

            for (int index = 0; index < steps.Count; index++)
            {
                Tween newTween = BuildTween(steps[index]);

                if (index == steps.Count - 1)
                {
                    newTween.OnComplete(SetResting);
                }
                else
                {
                    newTween.OnPlay(SetBusy);
                }

                _movementStore?.Add(newTween);
            }
        }

        public List<DoTweenMoveStep> GetSteps() => steps;
        public int GetStartPosition() => startPosition;

        public override void SetPosition()
        {
            if (activeProperty is EActiveProperty.Position)
            {
                activePart.localPosition = steps[startPosition].target;
            }
            else
            {
                activePart.localRotation = Quaternion.Euler(steps[startPosition].target);
            }

            CurrentMovement = startPosition == steps.Count - 1 ? 0 : startPosition + 1;
        }

        protected override void TriggerNext()
        {
            switch (moveType)
            {
                case ETriggerMoveType.ThereAndBack:
                    CurrentMovement = 0;
                    _movementStore[0].OnComplete(() => RunBackTween())
                        .Restart();
                    break;
                case ETriggerMoveType.Switch:
                    if (CurrentMovement == 0)
                    {
                        _movementStore[0].OnComplete(SetResting).Restart();
                        CurrentMovement = 1;
                    }
                    else
                    {
                        _movementStore[1].Restart();
                        CurrentMovement = 0;
                    }

                    break;
                case ETriggerMoveType.None:
                    break;
                case ETriggerMoveType.Multiple:
                    break;
                case ETriggerMoveType.Periodic:
                    break;
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
                ? activePart.DOLocalMove(target, duration)
                : activePart.DOLocalRotate(target, duration);

            result.SetEase(ease).SetAutoKill(false);

            return result;
        }
    }
}