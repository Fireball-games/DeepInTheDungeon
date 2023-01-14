using System;
using DG.Tweening;
using UnityEngine;

namespace Scripts.Triggers
{
    [Serializable]
    public class DoTweenMoveStep
    {
        public Vector3 target;
        public float duration = 0.3f;
        public Ease movementEase = Ease.Linear;
    }
}