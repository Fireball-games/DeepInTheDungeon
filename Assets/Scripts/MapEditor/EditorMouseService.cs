using System;
using Scripts.Building.Tile;
using Scripts.EventsManagement;
using Scripts.Helpers;
using Scripts.Helpers.Extensions;
using Scripts.System;
using Scripts.System.MonoBases;
using Scripts.UI.EditorUI;
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
        [SerializeField] private Cursor3D cursor3D;
        [SerializeField] private GameObject upperFloorTrigger;
        [SerializeField] private EditorCameraService cameraService;
        
        private static MapEditorManager Manager => MapEditorManager.Instance;
        private static EditorUIManager UIManager => EditorUIManager.Instance;

        public Vector3Int MouseGridPosition => _lastGridPosition;
        public EGridPositionType GridPositionType { get; private set; } = EGridPositionType.None;
        public bool LeftClickExpired { get; private set; }
        public bool RightClickExpired { get; private set; }
        public bool LeftClickedOnUI { get; private set; }

        private MapBuildService _buildService;
        private Plane _layerPlane;
        private Vector3Int _lastGridPosition;
        private readonly Vector3Int _invalidGridPosition = new(-10000, -10000, -10000);
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

        public void MoveCameraTo(float x, float y, float z) => cameraService.MoveCameraTo(x, y, z);

        protected override void Awake()
        {
            base.Awake();

            _buildService = new MapBuildService();
            
            _lastGridPosition = new Vector3Int(-1000, -1000, -1000);

            _demolishMouseHotspot = new Vector2(demolishCursor.width / 2, demolishCursor.height / 2);
            _moveCameraMouseHotspot = new Vector2(moveCameraCursor.width / 2, moveCameraCursor.height / 2);
        }

        private void OnEnable()
        {
            EditorEvents.OnNewMapStartedCreation += OnNewMapStartedCreation;
            EditorEvents.OnFloorChanged += OnFloorChanged;
        }

        private void Update()
        {
            if (!Manager.MapIsPresented || Manager.MapIsBeingBuilt) return;

            ValidateClicks();
            cameraService.HandleMouseMovement();
            cameraService.HandleMouseWheel();
            
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
                cameraService.HandleMouseWheel();
                SetGridPosition();
                cameraService.HandleMouseMovement();
            }
        }

        private void OnDisable()
        {
            EditorEvents.OnNewMapStartedCreation -= OnNewMapStartedCreation;
            EditorEvents.OnFloorChanged -= OnFloorChanged;
        }

        private void OnNewMapStartedCreation() => RecreateMousePlane();

        private void OnFloorChanged(int? _) => RecreateMousePlane();
        
        private void RecreateMousePlane() => _layerPlane = _layerPlane = new Plane(Vector3.up, 
            new Vector3(0f, 0.5f - Manager.CurrentFloor, 0f));

        private void ValidateClicks()
        {
            float currentTime = Time.time;

            if (Input.GetMouseButtonDown(0))
            {
                LeftClickExpired = false;
                _lastLeftClickTime = Time.time;

                LeftClickedOnUI = false;

                if (EventSystem.current.IsPointerOverGameObject())
                {
                    LeftClickedOnUI = true;
                }
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
                case EWorkMode.Walls:
                    if (mouseButtonUpped == 0)
                    {
                        
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void SetGridPosition(bool invalidateLastPosition = false)
        {
            if (IsManipulatingCameraPosition) return;

            if (!GetMousePlanePosition(out Vector3 worldPosition)) return;

            if (invalidateLastPosition)
            {
                _lastGridPosition = _invalidGridPosition;
            }

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
                Hide3DCursor();
                return;
            }

            bool isNullTile = layout.ByGridV3Int(newGridPosition) == null;
            
            ResolveBuildModePosition(isNullTile, newGridPosition, layout);
            ResolveWallModePosition(isNullTile);

            SetCursor(GridPositionType);
        }

        private void ResolveWallModePosition(bool isNullTile)
        {
            if (Manager.WorkMode != EWorkMode.Walls) return;
            
            GridPositionType = isNullTile ? EGridPositionType.NullTile : EGridPositionType.EditableTile;
        }

        internal void SetCursorToCameraMovement() => SetCursor(moveCameraCursor, _moveCameraMouseHotspot);

        private void ResolveBuildModePosition(bool isNullTile, Vector3Int newGridPosition,
            TileDescription[,,] layout)
        {
            if (Manager.WorkMode != EWorkMode.Build) return;
            
            if (Manager.WorkLevel == ELevel.Equal)
            {
                GridPositionType = isNullTile ? EGridPositionType.NullTile : EGridPositionType.EditableTile;
            }
            else if (Manager.WorkLevel == ELevel.Upper)
            {
                if (!isNullTile)
                {
                    Vector3Int aboveGridPosition = newGridPosition.AddToX(-1);

                    bool isNullTileAbove = layout.ByGridV3Int(aboveGridPosition) == null;
                        
                    GridPositionType = isNullTileAbove ? EGridPositionType.NullTileAbove : EGridPositionType.EditableTileAbove;
                    
                    upperFloorTrigger.transform.position = MouseGridPosition.ToWorldPosition();
                    upperFloorTrigger.SetActive(true);
                }
                else
                {
                    upperFloorTrigger.SetActive(false);
                    
                    _buildService.HandleUpperFloorVisibility();

                    GridPositionType = EGridPositionType.None;
                }
            }
            else if (Manager.WorkLevel == ELevel.Lower)
            {
                if (!isNullTile)
                {
                    Vector3Int bellowGridPosition = newGridPosition.AddToX(1);

                    bool isNullTileBellow = layout.ByGridV3Int(bellowGridPosition) == null;
                        
                    GridPositionType = isNullTileBellow ? EGridPositionType.NullTileBellow : EGridPositionType.EditableTileBellow;
                }
                else
                {
                    GridPositionType = EGridPositionType.None;
                }
            }
        }

        private void SetDefaultCursor()
        {
            if (_isDefaultCursorSet) return;

            Hide3DCursor();
            
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
            if (Manager.WorkMode == EWorkMode.Walls)
            {
                SetDefaultCursor();
            }
            
            if (Manager.WorkMode == EWorkMode.Build)
            {
                switch (type)
                {
                    case EGridPositionType.None:
                        Hide3DCursor();
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
                        Hide3DCursor();
                        SetDefaultCursor();
                        break;
                }
            }
        }

        public void RefreshMousePosition(bool invalidateLastPosition = false)
        {
            if (!Manager.MapIsPresented) return;

            SetGridPosition(invalidateLastPosition);
        }

        public void ResetCursor()
        {
            Hide3DCursor();
            
            SetDefaultCursor();
            RefreshMousePosition(true);
        }

        private void Hide3DCursor()
        {
            if (!UIManager.IsAnyObjectEdited)
            {
                cursor3D.Hide();
            }
        }
    }
}