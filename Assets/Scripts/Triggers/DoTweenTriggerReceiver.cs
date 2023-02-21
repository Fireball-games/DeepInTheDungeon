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

        private List<Tween> _positionStore;

        private void Awake()
        {
            _positionStore = new List<Tween>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            
            _positionStore?.Clear();

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

                _positionStore?.Add(newTween);
            }
        }

        public List<DoTweenMoveStep> GetSteps() => steps;
        public void Trigger() => TriggerNext();
        public int GetCurrentPosition() => CurrentPosition;
        public void SetCurrentPosition(int newPosition)
        {
            CurrentPosition = newPosition;
            
            if (activeProperty is EActiveProperty.Position)
            {
                activePart.localPosition = steps[CurrentPosition].target;
            }
            else
            {
                activePart.localRotation = Quaternion.Euler(steps[CurrentPosition].target);
            }
        }

        protected override void TriggerNext()
        {
            switch (moveType)
            {
                case ETriggerMoveType.ThereAndBack:
                    CurrentPosition = 0;
                    _positionStore[1].OnComplete(() => RunBackTween())
                        .Restart();
                    break;
                case ETriggerMoveType.Switch:
                    CurrentPosition = CurrentPosition == steps.Count - 1 ? 0 : CurrentPosition + 1;
                    _positionStore[CurrentPosition].OnComplete(() =>
                    {
                        CheckCorrectPosition();
                        SetResting();
                    }).Restart();

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

        private void CheckCorrectPosition()
        {
            if (activeProperty is EActiveProperty.Position)
            {
                activePart.localPosition = steps[CurrentPosition].target;
            }
            else
            {
                activePart.localRotation = Quaternion.Euler(steps[CurrentPosition].target);
            }
        }

        private void RunBackTween(bool triggerNextOnStart = false)
        {
            if (triggerNextOnStart)
            {
                TriggerNext();
            }

            _positionStore[0].OnComplete(() => {
                SetResting();
                _positionStore[0].OnComplete(null);
            }).Restart();
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