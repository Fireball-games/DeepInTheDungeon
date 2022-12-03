using System;
using System.Collections;
using Scripts.EventsManagement;
using UnityEngine;

namespace Scripts
{
    public class PlayerController : MonoBehaviour
    {
        public bool smoothTransition;
        public float transitionSpeed = 10f;
        public float wallBashSpeed = 1f;
        public float wallBashSpeedReturnMultiplier = 1.5f;
        public float transitionRotationSpeed = 500f;

        [SerializeField] private Camera playerCamera;

        public static float TransitionRotationSpeed { get; private set; }

        private Vector3 _targetGridPos;
        private Vector3 _prevTargetGridPos;
        private Vector3 _targetRotation;

        private bool _isStartPositionSet;
        private bool _isBashingIntoWall;
        private bool _atRest = true;

        private void Awake()
        {
            CameraManager.SetMainCamera(playerCamera);
        }

        public void SetPosition(Vector3 position)
        {
            transform.position = _targetGridPos = _prevTargetGridPos = position;
            _isStartPositionSet = true;
        }

        public void RotateLeft() => SetMovement(() => _targetRotation -= Vector3.up * 90f);
        public void RotateRight() => SetMovement(() => _targetRotation += Vector3.up * 90f);
        public void MoveForward() => SetMovement(() => _targetGridPos += transform.forward);
        public void MoveBackwards() => SetMovement(() => _targetGridPos -= transform.forward);
        public void MoveLeft() => SetMovement(() => _targetGridPos -= transform.right);
        public void MoveRight() => SetMovement(() => _targetGridPos += transform.right);

        private void SetMovement(Action movementSetter)
        {
            if (!_isStartPositionSet || !GameController.MovementEnabled || !_atRest) return;

            movementSetter?.Invoke();

            if (IsTargetPositionValid())
            {
                _atRest = false;
                _prevTargetGridPos = _targetGridPos;
                StartCoroutine(PerformMovementCoroutine());
                return;
            }

            if (!_isBashingIntoWall && _targetGridPos != _prevTargetGridPos)
            {
                _atRest = false;
                StartCoroutine(BashIntoWallCoroutine());
                return;
            }

            _targetGridPos = _prevTargetGridPos;
        }

        private IEnumerator PerformMovementCoroutine()
        {
            float currentRotY = transform.transform.eulerAngles.y;
            Vector3 currentPosition = transform.position;
            
            while (Vector3.Distance(transform.position, _targetGridPos) > 0.05f
                   || Vector3.Distance(transform.eulerAngles, _targetRotation) > 0.05)
            {
                Transform myTransform = transform;

                Vector3 targetPosition = _targetGridPos;

                if (_targetRotation.y is > 270f and < 361f) _targetRotation.y = 0f;
                if (_targetRotation.y < 0f) _targetRotation.y = 270f;

                if (!smoothTransition)
                {
                    transform.position = targetPosition;
                    transform.rotation = Quaternion.Euler(_targetRotation);
                }
                else
                {
                    transform.position = Vector3.MoveTowards(myTransform.position, targetPosition, Time.deltaTime * transitionSpeed);
                    transform.rotation = Quaternion.RotateTowards(myTransform.rotation, Quaternion.Euler(_targetRotation),
                        Time.deltaTime * transitionRotationSpeed);
                }

                yield return null;
            }

            _atRest = true;
            _prevTargetGridPos = _targetGridPos;
            
            if (Math.Abs(currentRotY - transform.rotation.eulerAngles.y) > float.Epsilon)
            {
                TransitionRotationSpeed = transitionRotationSpeed;
                EventsManager.TriggerOnPlayerRotationChanged(_targetRotation);
            }

            if (Vector3.Magnitude(transform.position - currentPosition) > 0.5f)
            {
                EventsManager.TriggerOnPlayerPositionChanged(transform.position);
            }
        }

        private IEnumerator BashIntoWallCoroutine()
        {
            Vector3 position = transform.position;
            Vector3 targetPosition = position + ((_targetGridPos - position) * 0.2f);

            while (Vector3.Distance(transform.position, targetPosition) > 0.05f)
            {
                transform.position = Vector3.MoveTowards(transform.position, _targetGridPos,
                    Time.deltaTime * wallBashSpeed);
                yield return null;
            }

            while (Vector3.Distance(transform.position, _prevTargetGridPos) > 0.05f)
            {
                transform.position = Vector3.MoveTowards(transform.position, _prevTargetGridPos,
                    Time.deltaTime * wallBashSpeed * wallBashSpeedReturnMultiplier);
                yield return null;
            }

            SetPosition(_prevTargetGridPos);
            _isBashingIntoWall = false;
            _atRest = true;
        }

        private bool IsTargetPositionValid()
        {
            Vector3Int intTargetPosition = Vector3Int.RoundToInt(_targetGridPos);
            return GameController.CurrentMapLayout[intTargetPosition.x, intTargetPosition.z] is {IsForMovement: true};
        }
    }
}