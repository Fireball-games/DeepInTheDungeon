using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using static Scripts.Enums;

namespace Scripts.Triggers
{
    public class DoTweenClickTrigger : MouseClickTrigger, IPositionsTrigger
    {
        public ETriggerMoveType moveType;
        public EActiveProperty activeProperty;
        public int startPosition;
        public List<DoTweenMoveStep> steps;

        private List<Tween> _positionStore;

        protected override void Awake()
        {
            base.Awake();
            _positionStore = new List<Tween>();
        }
        
        private void OnEnable()
        {
            _positionStore?.Clear();

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

                _positionStore?.Add(newTween);
            }
        }

        public  void SetPosition()
        {
            if (activeProperty is EActiveProperty.Position)
            {
                ActivePart.localPosition = steps[startPosition].target;
            }
            else
            {
                ActivePart.localRotation = Quaternion.Euler(steps[startPosition].target);
            }

            CurrentPosition = startPosition == steps.Count - 1 ? 0 : startPosition + 1;
        }

        protected override void OnTriggerActivated(ETriggerActivatedDetail _ = ETriggerActivatedDetail.None)
        {
            switch (moveType)
            {
                case ETriggerMoveType.ThereAndBack:
                    CurrentPosition = 0;
                    _positionStore[0].OnComplete(() => RunBackTween(true))
                        .Restart();
                    break;
                case ETriggerMoveType.Switch:
                    if (CurrentPosition == 0)
                    {
                        _positionStore[0].OnComplete(() => SetResting(true)).Restart();
                        CurrentPosition = 1;
                    }
                    else
                    {
                        _positionStore[1].OnComplete(() => SetResting(true)).Restart();
                        CurrentPosition = 0;
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

            if (count <= 0) return;

            _positionStore[^1].Restart();
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

        public List<DoTweenMoveStep> GetSteps() => steps;
        public int GetStartPosition() => startPosition;
        public void SetStartPosition(int newStartPosition) => startPosition = newStartPosition;
    }
}