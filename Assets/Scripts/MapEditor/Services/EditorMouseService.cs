using System;
using System.Collections.Generic;
using Scripts.Building.PrefabsSpawning;
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
using static Scripts.Enums;
using static Scripts.MapEditor.Enums;
using static Scripts.System.MouseCursorManager;

namespace Scripts.MapEditor.Services
{
    public class EditorMouseService : MouseServiceBase<EditorMouseService>
    {
        [SerializeField] private Cursor3D cursor3D;
        [SerializeField] private GameObject upperFloorTrigger;
        [SerializeField] private EditorCameraService cameraService;

        private static MapEditorManager Manager => MapEditorManager.Instance;
        private static EditorUIManager UIManager => EditorUIManager.Instance;

        public Vector3Int MouseGridPosition => _lastGridPosition;
        public Vector3Int LastLeftButtonUpWorldPosition { get; private set; }
        public EGridPositionType GridPositionType { get; private set; } = EGridPositionType.None;

        private MapBuildService _buildService;
        private Plane _layerPlane;
        private Vector3Int _lastGridPosition;
        private readonly Vector3Int _invalidGridPosition = new(-10000, -10000, -10000);

        private WallPrefabBase _lastEnteredWall;
        private GameObject _lastPrefabOnPosition;

        private readonly List<EWorkMode> _workModesForSimpleNullTileDetection = new()
        {
            EWorkMode.PrefabTiles,
            EWorkMode.EditEntryPoints,
            EWorkMode.SetWalls,
            EWorkMode.Walls,
            EWorkMode.EditEditorStart,
        };

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
            EditorEvents.OnWorkModeChanged += OnWorkModeChanged;
            EditorEvents.OnWorkingLevelChanged += OnWorkingLevelChanged;
        }

        private void Update()
        {
            if (!Manager || !Manager.MapIsPresented || Manager.MapIsBeingBuilt) return;

            CheckMouseOverWall();

            ValidateClicks();
            cameraService.HandleMouseMovement();

            if (EventSystem.current.IsPointerOverGameObject())
            {
                UIIsBlocking = true;
                SetDefaultCursor();

                return;
            }
            
            cameraService.HandleMouseWheel();

            if (UIIsBlocking)
            {
                RefreshMousePosition();
                UIIsBlocking = false;
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
                // cameraService.HandleMouseWheel();
                SetGridPosition();
                cameraService.HandleMouseMovement();
            }
        }

        private void OnDisable()
        {
            EditorEvents.OnNewMapStartedCreation -= OnNewMapStartedCreation;
            EditorEvents.OnFloorChanged -= OnFloorChanged;
            EditorEvents.OnWorkModeChanged -= OnWorkModeChanged;
            EditorEvents.OnWorkingLevelChanged -= OnWorkingLevelChanged;
        }

        public void RefreshMousePosition(bool invalidateLastPosition = false)
        {
            if (!Manager.MapIsPresented || UIManager.IsAnyObjectEdited) return;

            SetGridPosition(invalidateLastPosition);
        }

        public override void ResetCursor()
        {
            base.ResetCursor();
            
            RefreshMousePosition(true);
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
            EditorUIManager.Instance.TileGizmo.Reset();
        }

        private void OnNewMapStartedCreation() => RecreateMousePlane();

        private void OnFloorChanged(int? _) => RecreateMousePlane();

        private void OnWorkingLevelChanged(ELevel newLevel) => ResetCursor();

        private void OnWorkModeChanged(EWorkMode _) => ResetCursor();

        private void RecreateMousePlane() => _layerPlane = _layerPlane = new Plane(Vector3.up,
            new Vector3(0f, 0.5f - Manager.CurrentFloor, 0f));

        private void ProcessMouseButtonUp(int mouseButtonUpped)
        {
            LastLeftButtonUpWorldPosition = MouseGridPosition.ToWorldPositionV3Int();

            switch (Manager.WorkMode)
            {
                case EWorkMode.None:
                    break;
                case EWorkMode.Build:
                    if (mouseButtonUpped == 0 && GridPositionType != EGridPositionType.None)
                    {
                        _buildService.ProcessBuildTileClick();
                    }

                    break;
                case EWorkMode.Select:
                    break;
                case EWorkMode.Walls:
                    if (mouseButtonUpped == 0 && GridPositionType != EGridPositionType.None)
                    {
                        if (_lastEnteredWall is {WallEligibleForEditing: true}) _lastEnteredWall.OnClickInEditor();
                    }

                    break;
                case EWorkMode.PrefabTiles:
                    OpenEditorForTiledPrefab<TilePrefab>(mouseButtonUpped, EPrefabType.PrefabTile);
                    break;
                case EWorkMode.Prefabs:
                case EWorkMode.Items:
                case EWorkMode.Enemies:
                case EWorkMode.Triggers:
                    break;
                case EWorkMode.TriggerReceivers:
                    break;
                case EWorkMode.SetWalls:
                    break;
                case EWorkMode.EditEntryPoints:
                    OpenEditorForTiledPrefab<EntryPointPrefab>(mouseButtonUpped, EPrefabType.Service);
                    break;
                case EWorkMode.EditEditorStart:
                    if (mouseButtonUpped == 0 && GridPositionType == EGridPositionType.EditableTile)
                    {
                        UIManager.OpenEditorWindow(EPrefabType.Service,
                        new PositionRotation(MouseGridPosition.ToWorldPositionV3Int(), Quaternion.identity));
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OpenEditorForTiledPrefab<TPrefab>(int mouseButtonUpped, EPrefabType ePrefabType) where TPrefab : PrefabBase
        {
            if (mouseButtonUpped == 0 && GridPositionType != EGridPositionType.None)
            {
                if (_lastPrefabOnPosition && _lastPrefabOnPosition.GetComponent<TPrefab>())
                {
                    UIManager.OpenEditorWindow(Manager.MapBuilder.GetPrefabConfigurationByTransformData(
                        new PositionRotation(_lastPrefabOnPosition.transform.position, _lastPrefabOnPosition.transform.rotation)
                    ));
                }
                else if (!_lastPrefabOnPosition)
                {
                    UIManager.OpenEditorWindow(ePrefabType,
                        new PositionRotation(MouseGridPosition.ToWorldPositionV3Int(), Quaternion.identity));
                }
            }
        }

        private void SetGridPosition(bool invalidateLastPosition = false)
        {
            if (IsManipulatingCameraPosition || UIManager.IsAnyObjectEdited) return;

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
                SetCursor(ECursorType.Default);
                Hide3DCursor();
                return;
            }

            bool isNullTile = layout.ByGridV3Int(newGridPosition) == null;

            ResolveBuildModePosition(isNullTile, newGridPosition, layout);
            ResolveCommonSimpleModePosition(isNullTile);

            SetCursorByType(GridPositionType);
        }

        private void ResolveCommonSimpleModePosition(bool isNullTile)
        {
            if (!_workModesForSimpleNullTileDetection.Contains(Manager.WorkMode)) return;

            GridPositionType = isNullTile ? EGridPositionType.NullTile : EGridPositionType.EditableTile;
        }

        internal void SetCursorToCameraMovement() => SetCursor(ECursorType.Move);

        private void ResolveBuildModePosition(bool isNullTile, Vector3Int newGridPosition,
            TileDescription[,,] layout)
        {
            if (Manager.WorkMode != EWorkMode.Build) return;

            if (Manager.WorkLevel == ELevel.Equal)
            {
                upperFloorTrigger.SetActive(false);
                _buildService.HandleUpperFloorVisibility();

                GridPositionType = isNullTile ? EGridPositionType.NullTile : EGridPositionType.EditableTile;

                if (IsPositionOccupied(newGridPosition))
                {
                    GridPositionType = EGridPositionType.None;
                }
            }
            else if (Manager.WorkLevel == ELevel.Upper)
            {
                if (!isNullTile)
                {
                    Vector3Int aboveGridPosition = newGridPosition.AddToX(-1);

                    if (IsPositionOccupied(aboveGridPosition))
                    {
                        GridPositionType = EGridPositionType.None;
                        return;
                    }

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
                upperFloorTrigger.SetActive(false);
                _buildService.HandleUpperFloorVisibility();

                if (!isNullTile)
                {
                    Vector3Int bellowGridPosition = newGridPosition.AddToX(1);

                    if (IsPositionOccupied(bellowGridPosition))
                    {
                        GridPositionType = EGridPositionType.None;
                        return;
                    }

                    bool isNullTileBellow = layout.ByGridV3Int(bellowGridPosition) == null;

                    GridPositionType = isNullTileBellow ? EGridPositionType.NullTileBellow : EGridPositionType.EditableTileBellow;
                }
                else
                {
                    GridPositionType = EGridPositionType.None;
                }
            }
        }

        private bool IsPositionOccupied(Vector3Int newGridPosition)
        {
            return Manager.MapBuilder.GetPrefabByGridPosition(newGridPosition);
        }

        private void SetCursorByType(EGridPositionType type)
        {
            if (Manager.WorkMode is EWorkMode.Walls or EWorkMode.Triggers)
            {
                SetDefaultCursor();
            }

            if (Manager.WorkMode == EWorkMode.PrefabTiles)
            {
                SetCursorForTiledPrefabType<TilePrefab>(type);
            }
            
            if (Manager.WorkMode == EWorkMode.EditEntryPoints)
            {
                SetCursorForTiledPrefabType<EntryPointPrefab>(type);
            }
            
            if (Manager.WorkMode == EWorkMode.EditEditorStart)
            {
                if (GridPositionType == EGridPositionType.NullTile)
                {
                    SetDefaultCursor();
                }
                else
                {
                    Show3DCursor(MouseGridPosition);
                    SetCursor(ECursorType.Move);
                }
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
                        Show3DCursor(MouseGridPosition);
                        SetCursor(ECursorType.Build);
                        break;
                    case EGridPositionType.EditableTile:
                        Show3DCursor(MouseGridPosition);
                        SetCursor(ECursorType.Demolish);
                        break;
                    case EGridPositionType.NullTileAbove:
                        Show3DCursor(MouseGridPosition, true);
                        SetCursor(ECursorType.Build);
                        break;
                    case EGridPositionType.EditableTileAbove:
                        Show3DCursor(MouseGridPosition, true);
                        SetCursor(ECursorType.Demolish);
                        break;
                    case EGridPositionType.NullTileBellow:
                        Show3DCursor(MouseGridPosition, withCopyBellow: true);
                        SetCursor(ECursorType.Build);
                        break;
                    case EGridPositionType.EditableTileBellow:
                        Show3DCursor(MouseGridPosition, withCopyBellow: true);
                        SetCursor(ECursorType.Demolish);
                        break;
                    default:
                        ResetCursor();
                        break;
                }
            }
        }

        private void SetCursorForTiledPrefabType<TPrefab>(EGridPositionType type) where TPrefab : PrefabBase
        {
            GameObject prefabOnPosition = Manager.MapBuilder.GetPrefabByGridPosition(MouseGridPosition);
            bool isDesiredPrefab = prefabOnPosition && prefabOnPosition.GetComponent<TPrefab>();
            
            if (type == EGridPositionType.NullTile || prefabOnPosition && !isDesiredPrefab)
            {
                _lastPrefabOnPosition = null;
                cursor3D.Hide();
                SetCursor(ECursorType.Default);
                return;
            }

            if (prefabOnPosition)
            {
                _lastPrefabOnPosition = prefabOnPosition;
                    
                if (isDesiredPrefab)
                {
                    SetCursor(ECursorType.Edit);
                    Show3DCursor(MouseGridPosition);
                }
            }
            else
            {
                _lastPrefabOnPosition = null;
                SetCursor(ECursorType.Add);
                Show3DCursor(MouseGridPosition);
            }
        }

        private void Show3DCursor(Vector3Int gridPosition, bool withCopyBellow = false, bool withCopyAbove = false)
        {
            if (UIManager.IsAnyObjectEdited)
            {
                cursor3D.Hide();
                return;
            }
            
            cursor3D.ShowAt(gridPosition, withCopyAbove, withCopyBellow);
        }
    }
}