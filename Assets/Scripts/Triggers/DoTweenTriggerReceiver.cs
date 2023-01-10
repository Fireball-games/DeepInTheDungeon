using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Scripts.Triggers
{
    public class DoTweenTriggerReceiver : TriggerReceiver
    {
        public Enums.ETriggerMoveType moveType;
        public Enums.EActiveProperty activeProperty;
        public List<MovementStep> steps;

        private List<Tween> _movementStore;

        private void Awake()
        {
            _movementStore = new List<Tween>();
        }
        
        public override void SetMovementStep()
        {
            if (activeProperty is Enums.EActiveProperty.Position)
            {
                activePart.localPosition = steps[startMovement].target;
            }
            else
            {
                activePart.localRotation = Quaternion.Euler(steps[startMovement].target);
            }

            CurrentMovement = startMovement == steps.Count - 1 ? 0 : startMovement + 1;
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

        protected override void TriggerNext()
        {
            switch (moveType)
            {
                case Enums.ETriggerMoveType.ThereAndBack:
                    CurrentMovement = 0;
                    _movementStore[0].OnComplete(() => RunBackTween())
                        .Restart();
                    break;
                case Enums.ETriggerMoveType.Switch:
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

        private Tween BuildTween(MovementStep step)
            => BuildTween(step.target, step.duration, step.movementEase);

        private Tween BuildTween(Vector3 target, float duration, Ease ease = Ease.Linear)
        {
            Tween result = activeProperty is Enums.EActiveProperty.Position
                ? activePart.DOLocalMove(target, duration)
                : activePart.DOLocalRotate(target, duration);

            result.SetEase(ease).SetAutoKill(false);

            return result;
        }
    }

    [Serializable]
    public class MovementStep
    {
        public Vector3 target;
        public float duration = 0.3f;
        public Ease movementEase = Ease.Linear;
    }
}