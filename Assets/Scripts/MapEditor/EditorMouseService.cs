using System;
using Scripts.Building.Tile;
using Scripts.EventsManagement;
using Scripts.Helpers;
using Scripts.Helpers.Extensions;
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
        [SerializeField] private float maxZoomHeight = 20f;
        [SerializeField] private float cameraRotationSpeed = 100f;
        [SerializeField] private Transform cameraHolder;
        [SerializeField] private Cursor3D cursor3D;
        
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
        private bool _isDefaultCursorSet;
        private bool _uiIsBlocking;

        public bool IsManipulatingCameraPosition
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
            cursor3D.SetMapBuildService(_buildService);
            
            _lastGridPosition = new Vector3Int(-1000, -1000, -1000);

            _demolishMouseHotspot = new Vector2(demolishCursor.width / 2, demolishCursor.height / 2);
            _moveCameraMouseHotspot = new Vector2(moveCameraCursor.width / 2, moveCameraCursor.height / 2);
        }

        private void OnEnable()
        {
            EditorEvents.OnNewMapStartedCreation += OnNewMapStartedCreation;
        }

        private void Update()
        {
            if (!Manager.MapIsPresented || Manager.MapIsBeingBuilt) return;

            if (EventSystem.current.IsPointerOverGameObject())
            {
                _uiIsBlocking = true;
                SetDefaultCursor();
                return;
            }

            if (_uiIsBlocking)
            {
                RefreshMousePosition();
                _uiIsBlocking = false;
            }
            
            ValidateClicks();

            if (Input.GetMouseButtonUp(0))
            {
                RefreshMousePosition();
            }
            
            if (!LeftClickExpired && Input.GetMouseButtonUp(0))
            {   
                ProcessMouseButtonUp(0);
            }
            else
            {
                HandleMouseWheel();
                SetGridPosition();
                HandleMouseMovement();
            }
        }

        private void OnDisable()
        {
            EditorEvents.OnNewMapStartedCreation -= OnNewMapStartedCreation;
        }

        private void OnNewMapStartedCreation()
        {
            _layerPlane = _layerPlane = new Plane(Vector3.up, 
                new Vector3(0f, 0.5f - GameManager.Instance.CurrentMap.StartGridPosition.x, 0f));
        }

        private void TranslateCamera(float x, float y, float z)
        {
            _cameraMoveVector.x = z;
            _cameraMoveVector.y = y;
            _cameraMoveVector.z = -x;

            Vector3 newPosition = cameraHolder.position + _cameraMoveVector;

            newPosition.y = Mathf.Clamp(
                newPosition.y,
                -Manager.EditedLayout.Count + 3, maxZoomHeight);
            
            cameraHolder.position = newPosition;
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
                
                Matrix4x4 worldToLocalMatrix = transform.worldToLocalMatrix;
                Vector3 localForward = worldToLocalMatrix.MultiplyVector(-cameraHolder.forward);
                Vector3 localRight = worldToLocalMatrix.MultiplyVector(cameraHolder.right);
                Vector3 moveVector = localRight * (yDelta * Time.deltaTime * cameraPanSpeed);
                moveVector += localForward * (xDelta * Time.deltaTime * cameraPanSpeed); 
                
                cameraHolder.position += moveVector;
            }

            if (LeftClickExpired && Input.GetMouseButtonUp(0))
            {
                IsManipulatingCameraPosition = false;
            }

            if (RightClickExpired && Input.GetMouseButton(1))
            {
                IsManipulatingCameraPosition = true;
                SetCursorToCameraMovement();

                float xDelta = Input.GetAxis(Strings.MouseXAxis);
                float yDelta = Input.GetAxis(Strings.MouseYAxis);

                if (xDelta == 0f && yDelta == 0f) return;

                Vector3 cameraRotation = cameraHolder.localRotation.eulerAngles;
                _cameraMoveVector.x = cameraRotation.x;
                _cameraMoveVector.y = cameraRotation.y + (xDelta * Time.deltaTime * cameraRotationSpeed);
                _cameraMoveVector.z = cameraRotation.z - (yDelta * Time.deltaTime * cameraRotationSpeed);
                
                cameraHolder.localRotation = Quaternion.Euler(_cameraMoveVector);
            }
            
            if (LeftClickExpired && Input.GetMouseButtonUp(1))
            {
                IsManipulatingCameraPosition = false;
            }
        }

        private void SetGridPosition()
        {
            if (IsManipulatingCameraPosition) return;

            if (!GetMousePlanePosition(out Vector3 worldPosition)) return;

            Vector3Int newGridPosition = Vector3Int.RoundToInt(worldPosition);
            newGridPosition.y = newGridPosition.x;
            newGridPosition.x = Manager.CurrentFloor;

            if (!newGridPosition.Equals(_lastGridPosition))
            {
                EditorEvents.TriggerOnMouseGridPositionChanged(newGridPosition, _lastGridPosition);
                _lastGridPosition = newGridPosition;
                OnMouseGridPositionChanged(newGridPosition);
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

        private void OnMouseGridPositionChanged(Vector3Int newGridPosition)
        {
            TileDescription[,,] layout = GameManager.Instance.CurrentMap.Layout;

            GridPositionType = EGridPositionType.None;
            
            if (!layout.HasIndex(newGridPosition))
            {
                Cursor.SetCursor(null, _defaultMouseHotspot, CursorMode.Auto);
                cursor3D.Hide();
                return;
            }

            bool isNullTile = layout.ByGridV3int(newGridPosition) == null;
            
            if (Manager.WorkMode == EWorkMode.Build)
            {
                if (Manager.WorkLevel == ELevel.Equal)
                {
                    GridPositionType = isNullTile ? EGridPositionType.NullTile : EGridPositionType.EditableTile;
                }
                else if (Manager.WorkLevel == ELevel.Upper)
                {
                    if (!isNullTile)
                    {
                        Vector3Int aboveGridPosition = newGridPosition.AddToX(-1);

                        bool isNullTileAbove = layout.ByGridV3int(aboveGridPosition) == null;
                        
                        GridPositionType = isNullTileAbove ? EGridPositionType.NullTileAbove : EGridPositionType.EditableTileAbove;
                    
                        _buildService.ShowUpperLevelStoneCubesAround(aboveGridPosition);
                    }
                    else
                    {
                        GridPositionType = EGridPositionType.None;
                    }
                }
                else if (Manager.WorkLevel == ELevel.Lower)
                {
                    if (!isNullTile)
                    {
                        Vector3Int bellowGridPosition = newGridPosition.AddToX(1);

                        bool isNullTileBellow = layout.ByGridV3int(bellowGridPosition) == null;
                        
                        GridPositionType = isNullTileBellow ? EGridPositionType.NullTileBellow : EGridPositionType.EditableTileBellow;
                    }
                    else
                    {
                        GridPositionType = EGridPositionType.None;
                    }
                }
            }

            SetCursor(GridPositionType);
        }

        private void SetCursorToCameraMovement() => SetCursor(moveCameraCursor, _moveCameraMouseHotspot);

        private void SetDefaultCursor()
        {
            if (_isDefaultCursorSet) return;
            cursor3D.Hide();
            SetCursor(null, Vector3.zero);
            _isDefaultCursorSet = true;
        }

        private void SetCursor(Texture2D image, Vector3 hotspot)
        {
            Cursor.SetCursor(image, hotspot, CursorMode.Auto);
            _isDefaultCursorSet = false;
        }

        private void SetCursor(EGridPositionType type)
        {
            switch (type)
            {
                case EGridPositionType.None:
                    cursor3D.Hide();
                    SetDefaultCursor();
                    break;
                case EGridPositionType.NullTile:
                    cursor3D.ShowAt(MouseGridPosition);
                    SetCursor(digCursor, _defaultMouseHotspot);
                    break;
                case EGridPositionType.EditableTile:
                    cursor3D.ShowAt(MouseGridPosition);
                    SetCursor(demolishCursor, _demolishMouseHotspot);
                    break;
                case EGridPositionType.NullTileAbove:
                    cursor3D.ShowAt(MouseGridPosition, true);
                    SetCursor(digCursor, _defaultMouseHotspot);
                    break;
                case EGridPositionType.EditableTileAbove:
                    cursor3D.ShowAt(MouseGridPosition, true);
                    SetCursor(demolishCursor, _demolishMouseHotspot);
                    break;
                case EGridPositionType.NullTileBellow:
                    cursor3D.ShowAt(MouseGridPosition, withCopyBellow: true);
                    SetCursor(digCursor, _defaultMouseHotspot);
                    break;
                case EGridPositionType.EditableTileBellow:
                    cursor3D.ShowAt(MouseGridPosition, withCopyBellow: true);
                    SetCursor(demolishCursor, _demolishMouseHotspot);
                    break;
                default:
                    cursor3D.Hide();
                    SetDefaultCursor();
                    break;
            }
        }

        public void RefreshMousePosition()
        {
            if (!Manager.MapIsPresented) return;

            SetGridPosition();
        }

        public void ResetCursor()
        {
            cursor3D.Hide();
            SetDefaultCursor();
            RefreshMousePosition();
        }
    }
}