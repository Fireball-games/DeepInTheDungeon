using DG.Tweening;
using Scripts.EventsManagement;
using Scripts.Helpers;
using Scripts.System;
using Scripts.System.MonoBases;
using UnityEngine;
using static Scripts.System.MouseCursorManager;

namespace Scripts.MapEditor.Services
{
    public class EditorCameraService : SingletonNotPersisting<EditorCameraService>
    {
        public float minPanSpeed = 20f;
        public float maxPanSpeed = 100f;

        [Tooltip("If using smooth zoom, then its good about 8000, normal is good around 1500.")] [SerializeField]
        private float cameraZoomSpeed = 100f;

        [SerializeField] private float minZoomHeight = 2f;
        [SerializeField] private float maxZoomHeight = 20f;
        [SerializeField] private float cameraRotationSpeed = 100f;
        [SerializeField] private float prefabMoveCameraDistance = 8f;
        [SerializeField] private float prefabMoveCameraXOffset = -2f;
        [SerializeField] private Transform cameraHolder;

        public static bool IsOrthographic = true;
        
        private static bool _canManipulateView = true;
        public static bool CanManipulateView {
            get => _canManipulateView;
            set
            {
                if (!value)
                {
                    ResetCursor();
                }
                
                _canManipulateView = value;
            }
    }

        private EditorMouseService Mouse => EditorMouseService.Instance;
        private MapEditorManager Manager => MapEditorManager.Instance;
        private float FloorYPosition => -Manager.CurrentFloor;
        
        private Vector3 _cameraMoveVector = Vector3.zero;
        
        private Sequence _moveSequence;

        public PositionRotation GetCameraTransformData() => new(cameraHolder.transform.position, cameraHolder.transform.rotation);

        internal void ResetCamera()
        {
            PositionRotation cameraResetData = new(new Vector3(Manager.EditedLayout[0].Count / 2, Manager.EditedLayout.Count + 5f, Manager.EditedLayout[0][0].Count / 2), Quaternion.Euler(Vector3.zero));
            MoveCameraTo(cameraResetData);
            RotateCameraSmooth(Vector3.zero);
        }

        internal void RotateCameraSmooth(Vector3 targetAngles)
        {
            cameraHolder.transform.DOLocalRotate(targetAngles, 0.3f, RotateMode.FastBeyond360);
        }

        internal void HandleMouseMovement()
        {
            if (!CanManipulateView) return;
            
            if (Mouse.LeftClickExpired && !Input.GetMouseButton(0)
                || Mouse.RightClickExpired && !Input.GetMouseButton(1))
            {
                Mouse.IsManipulatingCameraPosition = false;
                Mouse.RefreshMousePosition(true);
                SetDefaultCursor();
            }
            
            if (!Mouse.LeftClickedOnUI && Mouse.LeftClickExpired && Input.GetMouseButton(0))
            {
               MoveCameraOnMouseDrag();
            }

            if (Mouse.RightClickExpired && Input.GetMouseButton(1))
            {
                RotateViewOnMouseDrag();
            }
        }

        private void MoveCameraOnMouseDrag()
        {
            Mouse.IsManipulatingCameraPosition = true;
            Mouse.SetCursorToCameraMovement();

            float xDelta = Input.GetAxis(Strings.MouseXAxis);
            float yDelta = Input.GetAxis(Strings.MouseYAxis);

            if (xDelta == 0f && yDelta == 0f) return;

            float minMaxHeightAlpha = Mathf.InverseLerp(FloorYPosition + minZoomHeight, FloorYPosition + maxZoomHeight,
                cameraHolder.transform.position.y);
            float zoomDependantPanSpeed = Mathf.Lerp(minPanSpeed, maxPanSpeed, minMaxHeightAlpha);
                
            Matrix4x4 worldToLocalMatrix = transform.worldToLocalMatrix;
            Vector3 localForward = worldToLocalMatrix.MultiplyVector(-cameraHolder.forward);
            Vector3 localRight = worldToLocalMatrix.MultiplyVector(cameraHolder.right);
            Vector3 moveVector = localRight * (yDelta * Time.deltaTime * zoomDependantPanSpeed);
            moveVector += localForward * (xDelta * Time.deltaTime * zoomDependantPanSpeed);

            cameraHolder.position += moveVector;
        }

        private void RotateViewOnMouseDrag()
        {
            Mouse.IsManipulatingCameraPosition = true;
            Mouse.SetCursorToCameraMovement();

            float xDelta = Input.GetAxis(Strings.MouseXAxis);
            float yDelta = Input.GetAxis(Strings.MouseYAxis);

            if (xDelta == 0f && yDelta == 0f) return;

            Vector3 cameraRotation = cameraHolder.localRotation.eulerAngles;
            _cameraMoveVector.x = cameraRotation.x;
            _cameraMoveVector.y = cameraRotation.y + (xDelta * Time.deltaTime * cameraRotationSpeed);
            _cameraMoveVector.z = cameraRotation.z - (yDelta * Time.deltaTime * cameraRotationSpeed);

            cameraHolder.localRotation = Quaternion.Euler(_cameraMoveVector);
        }

        internal void HandleMouseWheel()
        {
            float wheelDelta = Input.GetAxis(Strings.MouseWheel);

            if (wheelDelta != 0)
            {
                TranslateCamera(0, -wheelDelta * Time.deltaTime * cameraZoomSpeed, 0, false);
            }
        }

        public Vector3 MoveCameraToPrefab(Vector3 worldPosition, bool smooth = true)
        {
            MapEditorManager.Instance.SetFloor((int) -worldPosition.y);
            Vector3 moveVector = new(worldPosition.x,
                worldPosition.y + prefabMoveCameraDistance,
                worldPosition.z - prefabMoveCameraXOffset);

            MoveCameraTo(moveVector, smooth, true);

            return moveVector;
        }

        public void MoveCameraTo(PositionRotation positionRotation)
        {
            if (_moveSequence != null)
            {
                _moveSequence.Kill();
                _moveSequence = null;
            }
            
            _moveSequence = DOTween.Sequence().Append(cameraHolder.DORotate(positionRotation.Rotation.eulerAngles, 0.3f));
            _moveSequence.Insert(0, cameraHolder.DOMove(positionRotation.Position, 0.5f).SetEase(Ease.OutFlash));
            _moveSequence.Play();
        }

        private void MoveCameraTo(Vector3 worldPosition, bool smooth = true, bool resetCameraAngle = false)
        {
            if (smooth)
            {
                if (resetCameraAngle)
                {
                    cameraHolder.DORotate(Vector3.zero, 0.3f).Play().SetAutoKill(true);
                }

                cameraHolder.DOMove(worldPosition, 0.5f).SetEase(Ease.OutFlash).Play().SetAutoKill(true);
            }
            else
            {
                cameraHolder.position = worldPosition;
            }
        }


        internal void MoveCameraTo(float x, float y, float z, bool smooth = true, bool resetCameraAngle = false)
        {
            _cameraMoveVector.x = x;
            _cameraMoveVector.y = y;
            _cameraMoveVector.z = z;

            MoveCameraTo(_cameraMoveVector, smooth, resetCameraAngle);
        }

        internal void TranslateCamera(Vector3 positionDelta, bool smooth = true)
            => TranslateCamera(positionDelta.x, positionDelta.y, positionDelta.z, smooth);

        internal static void ToggleCameraPerspective()
        {
            IsOrthographic = !CameraManager.Instance.mainCamera.orthographic;
            CameraManager.Instance.mainCamera.orthographic = IsOrthographic;
            EditorEvents.TriggerOnCameraPerspectiveChanged(IsOrthographic);
        }

        private void TranslateCamera(float x, float y, float z, bool smooth = true)
        {
            _cameraMoveVector.x = z;
            _cameraMoveVector.y = y;
            _cameraMoveVector.z = -x;

            Vector3 newPosition = cameraHolder.position + _cameraMoveVector;

            newPosition.y = Mathf.Clamp(newPosition.y,FloorYPosition + minZoomHeight, FloorYPosition + maxZoomHeight);

            if (smooth)
            {
                cameraHolder.DOMove(newPosition, 0.5f).SetEase(Ease.OutFlash).Play().SetAutoKill(true);
            }
            else
            {
                cameraHolder.position = newPosition;
            }
        }
    }
}