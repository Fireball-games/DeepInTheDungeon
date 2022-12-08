using System;
using Scripts.Building.Tile;
using Scripts.EventsManagement;
using Scripts.Helpers;
using Scripts.System;
using UnityEngine;
using UnityEngine.EventSystems;
using static Scripts.MapEditor.Enums;

namespace Scripts.MapEditor
{
    public class EditorMouseService : SingletonNotPersisting<EditorMouseService>
    {
        [SerializeField] private Texture2D digCursor;
        [SerializeField] private Texture2D demolishCursor;
        [SerializeField] private Texture2D moveCameraCursor;
        [SerializeField] private float maxValidClickTime = 0.1f;
        [SerializeField] private float cameraPanSpeed = 100f;
        [SerializeField] private float cameraZoomSpeed = 100f;
        [SerializeField] private Transform cameraHolder;
        private static MapEditorManager Manager => MapEditorManager.Instance;

        public Vector3Int MouseGridPosition => _lastGridPosition;
        public EGridPositionType GridPositionType { get; private set; } = EGridPositionType.None;
        public bool LeftClickExpired { get; private set; }
        public bool RightClickExpired { get; private set; }

        private MapBuildService _buildService;
        private Plane _layerPlane;
        private Vector3Int _lastGridPosition;
        private readonly Vector2 _defaultMouseHotspot = Vector2.zero;
        private Vector2 _demolishMouseHotspot;
        private Vector2 _moveCameraMouseHotspot;

        private float _lastLeftClickTime;
        private float _lastRightClickTime;

        private bool _isManipulatingCameraPosition;

        private bool IsManipulatingCameraPosition
        {
            get => _isManipulatingCameraPosition;
            set
            {
                if (value == _isManipulatingCameraPosition) return;

                if (!_isManipulatingCameraPosition)
                {
                    ResetCursor();
                }

                _isManipulatingCameraPosition = value;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            _buildService = new MapBuildService();

            _layerPlane = new Plane(Vector3.up, new Vector3(0f, 0.5f, 0f));
            _lastGridPosition = new Vector3Int(-1000, -1000, -1000);

            _demolishMouseHotspot = new Vector2(demolishCursor.width / 2, demolishCursor.height / 2);
            _moveCameraMouseHotspot = new Vector2(moveCameraCursor.width / 2, moveCameraCursor.height / 2);
        }

        private void Update()
        {
            if (!Manager.MapIsPresented || EventSystem.current.IsPointerOverGameObject()) return;

            ValidateClicks();

            if (!LeftClickExpired && Input.GetMouseButtonUp(0))
            {
                ProcessMouseButtonUp(0);
            }

            HandleMouseWheel();
            SetGridPosition();
            HandleMouseMovement();
        }

        private void TranslateCameraHorizontal(float x, float z) => TranslateCamera(x, 0, z);

        private void TranslateCamera(float x, float y, float z)
        {
            _cameraMoveVector.x = z;
            _cameraMoveVector.y = y;
            _cameraMoveVector.z = -x;

            cameraHolder.Translate(_cameraMoveVector);
        }

        public void MoveCameraTo(float x, float y, float z)
        {
            _cameraMoveVector.x = x;
            _cameraMoveVector.y = y;
            _cameraMoveVector.z = z;

            cameraHolder.position = _cameraMoveVector;
        }

        private void ValidateClicks()
        {
            float currentTime = Time.time;

            if (Input.GetMouseButtonDown(0))
            {
                LeftClickExpired = false;
                _lastLeftClickTime = Time.time;
            }

            if (Input.GetMouseButtonDown(1))
            {
                RightClickExpired = false;
                _lastRightClickTime = Time.time;
            }

            if (!LeftClickExpired && currentTime - _lastLeftClickTime > maxValidClickTime)
            {
                LeftClickExpired = true;
            }

            if (!RightClickExpired && currentTime - _lastRightClickTime > maxValidClickTime)
            {
                RightClickExpired = true;
            }
        }

        private void HandleMouseWheel()
        {
            float wheelDelta = Input.GetAxis(Strings.MouseWheel);
            if (wheelDelta != 0)
            {
                TranslateCamera(0, -wheelDelta * Time.deltaTime * cameraZoomSpeed, 0);
            }
        }

        private void ProcessMouseButtonUp(int mouseButtonUpped)
        {
            switch (Manager.WorkMode)
            {
                case EWorkMode.None:
                    break;
                case EWorkMode.Build:
                    if (mouseButtonUpped == 0)
                    {
                        _buildService.ProcessBuildClick();
                    }

                    break;
                case EWorkMode.Select:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private Vector3 _cameraMoveVector = Vector3.zero;

        private void HandleMouseMovement()
        {
            if (LeftClickExpired && Input.GetMouseButton(0))
            {
                IsManipulatingCameraPosition = true;
                SetCursorToCameraMovement();

                float xDelta = Input.GetAxis(Strings.MouseXAxis);
                float yDelta = Input.GetAxis(Strings.MouseYAxis);

                if (xDelta == 0f && yDelta == 0f) return;

                _cameraMoveVector.x = yDelta;
                _cameraMoveVector.z = -xDelta;

                _cameraMoveVector *= Time.deltaTime * cameraPanSpeed;

                TranslateCameraHorizontal(xDelta * Time.deltaTime * cameraPanSpeed, yDelta * Time.deltaTime * cameraPanSpeed);
            }

            if (LeftClickExpired && Input.GetMouseButtonUp(0))
            {
                IsManipulatingCameraPosition = false;
            }
        }

        private void SetGridPosition()
        {
            if (IsManipulatingCameraPosition) return;

            if (!GetMousePlanePosition(out Vector3 position)) return;

            Vector3Int newGridPosition = position.ToVector3Int();

            if (!newGridPosition.Equals(_lastGridPosition))
            {
                EditorEvents.TriggerOnMouseGridPositionChanged(newGridPosition, _lastGridPosition);
                OnMouseGridPositionChanged(newGridPosition);
                _lastGridPosition = newGridPosition;
            }
        }

        private bool GetMousePlanePosition(out Vector3 mousePlanePosition)
        {
            Ray ray = CameraManager.Instance.mainCamera.ScreenPointToRay(Input.mousePosition);
            if (_layerPlane.Raycast(ray, out float distance))
            {
                mousePlanePosition = ray.GetPoint(distance);
                return true;
            }

            mousePlanePosition = Vector3.negativeInfinity;
            return false;
        }

        private void OnMouseGridPositionChanged(Vector3Int newPosition)
        {
            TileDescription[,,] layout = GameManager.Instance.CurrentMap.Layout;

            if (!layout.HasIndex(newPosition))
            {
                Cursor.SetCursor(null, _defaultMouseHotspot, CursorMode.Auto);
                GridPositionType = EGridPositionType.None;
                return;
            }

            bool isNullTile = layout[newPosition.x, newPosition.y, newPosition.z] == null;

            GridPositionType = isNullTile ? EGridPositionType.Null : EGridPositionType.EditableTile;

            Texture2D newCursor = null;
            Vector2 hotspot = _defaultMouseHotspot;

            if (Manager.WorkMode == EWorkMode.Build)
            {
                newCursor = isNullTile ? digCursor : demolishCursor;
                hotspot = isNullTile ? _defaultMouseHotspot : _demolishMouseHotspot;
            }

            SetCursor(newCursor, hotspot);
        }

        private void SetCursorToCameraMovement() => SetCursor(moveCameraCursor, _moveCameraMouseHotspot);

        private void SetCursor(Texture2D image, Vector3 hotspot)
        {
            Cursor.SetCursor(image, hotspot, CursorMode.Auto);
        }

        public void RefreshMousePosition()
        {
            if (!MapEditorManager.Instance.MapIsPresented) return;

            OnMouseGridPositionChanged(MouseGridPosition);
        }

        public void ResetCursor()
        {
            SetCursor(null, Vector3.zero);
            RefreshMousePosition();
        }
    }
}