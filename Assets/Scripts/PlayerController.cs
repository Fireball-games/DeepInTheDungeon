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

        private bool AtRest =>
            Vector3.Distance(transform.position, _targetGridPos) < 0.05f
            && Vector3.Distance(transform.eulerAngles, _targetRotation) < 0.05f;

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

        public void RotateLeft() { if (AtRest) _targetRotation -= Vector3.up * 90f; }
        public void RotateRight() { if (AtRest) _targetRotation += Vector3.up * 90f; }

        public void MoveForward()
        {
            if (AtRest) _targetGridPos += transform.forward;
        }
        public void MoveBackwards() { if (AtRest) _targetGridPos -= transform.forward; }
        public void MoveLeft() { if (AtRest) _targetGridPos -= transform.right; }
        public void MoveRight() { if (AtRest) _targetGridPos += transform.right; }

        private bool IsTargetPositionValid()
        {
            Vector3Int intTargetPosition = Vector3Int.RoundToInt(_targetGridPos);
            return GameController.CurrentMapLayout[intTargetPosition.x][intTargetPosition.z] != 0;
        }
    }
}
