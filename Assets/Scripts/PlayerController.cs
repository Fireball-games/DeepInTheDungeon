using System;
using System.Collections;
using UnityEngine;

namespace Scripts
{
    public class PlayerController : MonoBehaviour
    {
        public bool smoothTransition;
        public float transitionSpeed = 10f;
        public float transitionRotationSpeed = 500f;

        private Vector3 _targetGridPos;
        private Vector3 _prevTargetGridPos;
        private Vector3 _targetRotation;

        private bool _isStartPositionSet;
        private bool _isBashingIntoWall;

        private bool AtRest = true;//=> !_isBashingIntoWall
            // && Vector3.Distance(transform.position, _targetGridPos) < 0.05f
            // && Vector3.Distance(transform.eulerAngles, _targetRotation) < 0.05f;

        private void FixedUpdate()
        {
            if (_isStartPositionSet)
            {
                MovePlayer();
            }
        }

        public void SetPosition(Vector3 position)
        {
            transform.position = _targetGridPos = _prevTargetGridPos = position;
            _isStartPositionSet = true;
        }

        private void MovePlayer()
        {
            if (IsTargetPositionValid())
            {
                _prevTargetGridPos = _targetGridPos;
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
            }
            else
            {
                _targetGridPos = _prevTargetGridPos;
            }
        }

        public void RotateLeft() => SetMovement(() => _targetRotation -= Vector3.up * 90f);
        public void RotateRight() => SetMovement(() => _targetRotation += Vector3.up * 90f);
        public void MoveForward() => SetMovement(() => _targetGridPos += transform.forward);
        public void MoveBackwards() => SetMovement(() => _targetGridPos -= transform.forward);
        public void MoveLeft() => SetMovement(() => _targetGridPos -= transform.right);
        public void MoveRight() => SetMovement(() => _targetGridPos += transform.right);

        private void SetMovement(Action movementSetter)
        {
            if (_isStartPositionSet && AtRest && IsTargetPositionValid())
            {
                AtRest = false;
                _prevTargetGridPos = _targetGridPos;
                movementSetter?.Invoke();
                StartCoroutine(PerformMovementCoroutine());
            }
            else
            {
                _targetGridPos = _prevTargetGridPos;
            }
        }

        private IEnumerator PerformMovementCoroutine()
        {
            while (Vector3.Distance(transform.position, _targetGridPos) < 0.05f
                   && Vector3.Distance(transform.eulerAngles, _targetRotation) < 0.05)
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

            AtRest = true;
        }

        private bool IsTargetPositionValid()
        {
            Vector3Int intTargetPosition = Vector3Int.RoundToInt(_targetGridPos);
            return GameController.CurrentMapLayout[intTargetPosition.x][intTargetPosition.z] != 0;
        }
    }
}
