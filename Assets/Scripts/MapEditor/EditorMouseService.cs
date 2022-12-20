using System;
using Scripts.Building;
using Scripts.Building.PrefabsSpawning.Walls;
using Scripts.Building.Tile;
using Scripts.EventsManagement;
using Scripts.Helpers;
using Scripts.Helpers.Extensions;
using Scripts.System;
using Scripts.System.MonoBases;
using Scripts.UI.EditorUI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using static Scripts.MapEditor.Enums;
using static Scripts.System.MouseCursorManager;

namespace Scripts.MapEditor
{
    public class EditorMouseService : SingletonNotPersisting<EditorMouseService>
    {
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
        
        private WallPrefabBase _lastEnteredWall;
        private GameObject _lastPrefabOnPosition;

        private float _lastLeftClickTime;
        private float _lastRightClickTime;

        private bool _isManipulatingCameraPosition;
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
        }

        private void OnEnable()
        {
            EditorEvents.OnNewMapStartedCreation += OnNewMapStartedCreation;
            EditorEvents.OnFloorChanged += OnFloorChanged;
        }

        private void Update()
        {
            if (!Manager.MapIsPresented || Manager.MapIsBeingBuilt) return;

            CheckMouseOverWall();

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
        
        private void CheckMouseOverWall()
        {
            if (Manager.WorkMode != EWorkMode.Walls || EditorUIManager.Instance.IsAnyObjectEdited) return;
            
            if (LayersManager.CheckRayHit(LayersManager.WallMaskName, out GameObject hitWall))
            {
                WallPrefabBase wall = hitWall.GetComponentInParent<WallPrefabBase>();
                if (wall)
                {
                    _lastEnteredWall = wall;
                    wall.OnMouseEntered();
                    return;
                }
            }

            if (!_lastEnteredWall) return;
            
            _lastEnteredWall.WallEligibleForEditing = false;
            _lastEnteredWall = null;
            cursor3D.Hide();
            EditorUIManager.Instance.WallGizmo.Reset();
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
                        _buildService.ProcessBuildTileClick();
                    }

                    break;
                case EWorkMode.Select:
                    break;
                case EWorkMode.Walls:
                    if (mouseButtonUpped == 0)
                    {
                        if (_lastEnteredWall is {WallEligibleForEditing: true}) _lastEnteredWall.OnClickInEditor();
                    }

                    break;
                case EWorkMode.PrefabTiles:
                    if (mouseButtonUpped == 0)
                    {
                        if (_lastPrefabOnPosition)
                        {
                            UIManager.OpenEditorWindow(Manager.MapBuilder.GetPrefabConfigurationByTransformData(
                                new PositionRotation(_lastPrefabOnPosition.transform.position, _lastPrefabOnPosition.transform.rotation)
                            ));
                        }
                        else
                        {
                            UIManager.OpenEditorWindow(Scripts.Enums.EPrefabType.PrefabTile,
                                new PositionRotation(MouseGridPosition.ToWorldPositionV3Int(), Quaternion.identity));
                        }
                    }
                    
                    break;
                case EWorkMode.Prefabs:
                case EWorkMode.Items:
                case EWorkMode.Enemies:
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
            ResolvePrefabTileModePosition(isNullTile);

            SetCursorByType(GridPositionType);
        }

        private void ResolveWallModePosition(bool isNullTile)
        {
            if (Manager.WorkMode != EWorkMode.Walls) return;
            
            GridPositionType = isNullTile ? EGridPositionType.NullTile : EGridPositionType.EditableTile;
        }
        
        private void ResolvePrefabTileModePosition(bool isNullTile)
        {
            if (Manager.WorkMode != EWorkMode.PrefabTiles) return;
            
            GridPositionType = isNullTile ? EGridPositionType.NullTile : EGridPositionType.EditableTile;
        }

        internal void SetCursorToCameraMovement() => SetCursor(ECursorType.Move);

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

        private void SetCursorByType(EGridPositionType type)
        {
            if (Manager.WorkMode == EWorkMode.Walls)
            {
                SetDefaultCursor();
            }

            if (Manager.WorkMode == EWorkMode.PrefabTiles)
            {
                if (type == EGridPositionType.NullTile)
                {
                    _lastPrefabOnPosition = null;
                    cursor3D.Hide();
                    SetCursor(ECursorType.Default);
                    return;
                }

                GameObject prefabOnPosition = Manager.MapBuilder.GetPrefabByGridPosition(MouseGridPosition); 
                if (prefabOnPosition)
                {
                    _lastPrefabOnPosition = prefabOnPosition;
                    SetCursor(ECursorType.Edit);
                }
                else
                {
                    _lastPrefabOnPosition = null;
                    SetCursor(ECursorType.Add);
                }
                
                cursor3D.ShowAt(MouseGridPosition);
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
                        SetCursor(ECursorType.Build);
                        break;
                    case EGridPositionType.EditableTile:
                        cursor3D.ShowAt(MouseGridPosition);
                        SetCursor(ECursorType.Demolish);
                        break;
                    case EGridPositionType.NullTileAbove:
                        cursor3D.ShowAt(MouseGridPosition, true);
                        SetCursor(ECursorType.Build);
                        break;
                    case EGridPositionType.EditableTileAbove:
                        cursor3D.ShowAt(MouseGridPosition, true);
                        SetCursor(ECursorType.Demolish);
                        break;
                    case EGridPositionType.NullTileBellow:
                        cursor3D.ShowAt(MouseGridPosition, withCopyBellow: true);
                        SetCursor(ECursorType.Build);
                        break;
                    case EGridPositionType.EditableTileBellow:
                        cursor3D.ShowAt(MouseGridPosition, withCopyBellow: true);
                        SetCursor(ECursorType.Demolish);
                        break;
                    default:
                        ResetCursor();
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
            MouseCursorManager.ResetCursor();
            RefreshMousePosition(true);
        }
    }
}