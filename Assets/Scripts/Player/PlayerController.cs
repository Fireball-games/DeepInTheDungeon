using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Scripts.Building.PrefabsSpawning.Configurations;
using Scripts.Building.PrefabsSpawning.Walls;
using Scripts.Building.PrefabsSpawning.Walls.Indentificators;
using Scripts.EventsManagement;
using Scripts.Helpers;
using Scripts.Helpers.Extensions;
using Scripts.ScriptableObjects;
using Scripts.System;
using UnityEngine;

namespace Scripts.Player
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

        private Vector3 _targetPosition;
        private Vector3 _prevTargetPosition;
        private Vector3 _targetRotation;

        private List<Waypoint> _waypoints;

        private bool _isStartPositionSet;
        private bool _isBashingIntoWall;
        private bool _atRest = true;

        public void SetPositionAndRotation(Vector3 gridPosition, Quaternion rotation)
        {
            Transform playerTransform = transform;
            playerTransform.position = _targetPosition = _prevTargetPosition = new Vector3(gridPosition.y, -gridPosition.x, gridPosition.z);
            playerTransform.rotation = rotation;
            _targetRotation = rotation.eulerAngles;
            _isStartPositionSet = true;
            _waypoints = null;

            StartCoroutine(GroundCheckCoroutine(true));
        }

        public void RotateLeft() => SetMovement(() => _targetRotation -= Vector3.up * 90f);
        public void RotateRight() => SetMovement(() => _targetRotation += Vector3.up * 90f);
        public void MoveForward() => SetMovement(() => _targetPosition += transform.forward);
        public void MoveBackwards() => SetMovement(() => _targetPosition -= transform.forward);
        public void MoveLeft() => SetMovement(() => _targetPosition -= transform.right);
        public void MoveRight() => SetMovement(() => _targetPosition += transform.right);

        private void SetMovement(Action movementSetter)
        {
            if (!_isStartPositionSet || !GameManager.Instance.MovementEnabled || !_atRest) return;

            movementSetter?.Invoke();

            if (IsTargetPositionValid())
            {
                _atRest = false;

                if (_waypoints == null || !_waypoints.Any())
                {
                    _prevTargetPosition = _targetPosition;
                    StartCoroutine(PerformMovementCoroutine());
                    return;
                }
            }
            else if (_waypoints != null && _waypoints.Any())
            {
                StartCoroutine(PerformWaypointMovementCoroutine());
                return;
            }

            if (!_isBashingIntoWall && (_targetPosition != _prevTargetPosition))
            {
                _atRest = false;
                StartCoroutine(BashIntoWallCoroutine());
                return;
            }

            _targetPosition = _prevTargetPosition;
        }

        private IEnumerator PerformWaypointMovementCoroutine()
        {
            for (int index = 1; index < _waypoints.Count; index++)
            {
                Waypoint waypoint = _waypoints[index];

                _targetPosition = waypoint.position;
                if (index == 1)
                {
                    _targetRotation = Quaternion.LookRotation(waypoint.position - _waypoints[0].position, Vector3.up).eulerAngles;
                }
                else if (index != _waypoints.Count - 1)
                {
                    _targetRotation = Quaternion.LookRotation(_waypoints[index + 1].position - transform.position, Vector3.up).eulerAngles;
                }
                else if (index == _waypoints.Count - 1)
                {
                    _targetRotation = Quaternion.LookRotation(_waypoints[^1].position - _waypoints[^2].position, Vector3.up).eulerAngles;
                }

                yield return PerformMovementCoroutine();
            }

            Vector3 rotation = transform.rotation.eulerAngles;
            SetPositionAndRotation(_waypoints[^1].position.ToGridPosition(), Quaternion.Euler(new Vector3(0, rotation.y, 0)));
            _waypoints = null;
        }

        private IEnumerator PerformMovementCoroutine()
        {
            Transform myTransform = transform;
            float currentRotY = myTransform.eulerAngles.y;
            Vector3 currentPosition = myTransform.position;

            while (NotAtTargetPosition(_targetPosition) || NotAtTargetRotation())
            {
                Vector3 targetPosition = _targetPosition;

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

            yield return GroundCheckCoroutine();

            _atRest = true;
            _prevTargetPosition = _targetPosition;

            if (Math.Abs(currentRotY - transform.rotation.eulerAngles.y) > float.Epsilon)
            {
                TransitionRotationSpeed = transitionRotationSpeed;
                EventsManager.TriggerOnPlayerRotationChanged(_targetRotation);
            }

            if (NotAtTargetPosition(currentPosition))
            {
                EventsManager.TriggerOnPlayerPositionChanged(transform.position);
            }
        }

        private IEnumerator GroundCheckCoroutine(bool setAtRestAtTheEnd = false)
        {
            while (!GroundCheck())
            {
                _atRest = false;

                Vector3 targetPosition = transform.position + Vector3.down;

                while (NotAtTargetPosition(targetPosition))
                {
                    transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * transitionSpeed);
                    yield return null;
                }

                Transform ownTransform = transform;
                ownTransform.position = _targetPosition = _prevTargetPosition = transform.position.ToVector3Int();

                yield return null;
            }

            if (setAtRestAtTheEnd) _atRest = true;
        }

        private IEnumerator BashIntoWallCoroutine()
        {
            Vector3 position = transform.position;
            Vector3 targetPosition = position + ((_targetPosition - position) * 0.2f);

            while (NotAtTargetPosition(targetPosition))
            {
                transform.position = Vector3.MoveTowards(transform.position, _targetPosition,
                    Time.deltaTime * wallBashSpeed);
                yield return null;
            }

            while (NotAtTargetPosition(_prevTargetPosition))
            {
                transform.position = Vector3.MoveTowards(transform.position, _prevTargetPosition,
                    Time.deltaTime * wallBashSpeed * wallBashSpeedReturnMultiplier);
                yield return null;
            }

            Vector3Int newPosition = Vector3Int.RoundToInt(transform.position).SwapXY();
            newPosition.x = 0 - newPosition.x;

            SetPositionAndRotation(newPosition, Quaternion.Euler(_targetRotation));
            _isBashingIntoWall = false;
            _atRest = true;
        }

        private bool IsTargetPositionValid()
        {
            Vector3Int intTargetPosition = Vector3Int.RoundToInt(_targetPosition);
            intTargetPosition.y = -intTargetPosition.y;

            return !IsMidWallInTargetDirection()
                   && GameManager.Instance.CurrentMap.Layout[intTargetPosition.y, intTargetPosition.x, intTargetPosition.z] is {IsForMovement: true};
        }

        private bool IsMidWallInTargetDirection()
        {
            if (_targetPosition == _prevTargetPosition) return false;

            Vector3 currentPosition = transform.position;
            Ray ray = new(currentPosition, _targetPosition - currentPosition);
            RaycastHit[] hits = new RaycastHit[5];
            int size = Physics.RaycastNonAlloc(ray, hits, 0.7f, LayerMask.GetMask(LayersManager.WallMaskName));

            if (size == 0) return false;

            foreach (RaycastHit hit in hits)
            {
                if (!hit.collider) continue;

                WallPrefabBase wallScript = hit.collider.gameObject.GetComponent<WallPrefabBase>();

                if (!wallScript) continue;

                if (wallScript is IObstacle) return true;

                if (wallScript is IMovementWall)
                {
                    Transform hitTransform = hit.transform;
                    _waypoints = (GameManager.Instance.MapBuilder
                        .GetPrefabConfigurationByTransformData(
                            new PositionRotation(hitTransform.position, hitTransform.rotation)) as WallConfiguration)?.WayPoints;
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if bellow player position is collider with wall tag and if not, returns true (as ground is there - check).
        /// </summary>
        /// <returns>False means player will fall</returns>
        private bool GroundCheck()
        {
            Vector3 currentPosition = transform.position;
            Ray ray = new(currentPosition, Vector3.down);
            return Physics.Raycast(ray, 0.7f, LayerMask.GetMask(LayersManager.WallMaskName));
        }

        public void SetCamera()
        {
            CameraManager.Instance.SetMainCamera(playerCamera);
        }

        private bool NotAtTargetPosition(Vector3 targetPosition) => Vector3.Distance(transform.position, targetPosition) > 0.05f;

        private bool NotAtTargetRotation() => Vector3.Distance(transform.eulerAngles, _targetRotation) > 0.05;
    }
}