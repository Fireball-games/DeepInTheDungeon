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
    public partial class EditorMouseService : MouseServiceBase<EditorMouseService>
    {
        [SerializeField] private Cursor3D cursor3D;
        [SerializeField] private GameObject upperFloorTrigger;
        [SerializeField] private EditorCameraService cameraService;

        private static MapEditorManager Manager => MapEditorManager.Instance;
        private static EditorUIManager UIManager => EditorUIManager.Instance;

        public Vector3Int MouseGridPosition => _lastGridPosition;
        public Vector3Int LastLeftButtonUpWorldPosition { get; private set; }
        public Vector3 MousePositionOnPlane { get; private set; }
        public EGridPositionType GridPositionType { get; private set; } = EGridPositionType.None;
        public bool IsOverUI { get; private set; }

        private MapBuildService _buildService;
        private ItemsMouseService _itemsService;
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
            EWorkMode.Items,
        };

        protected override void Awake()
        {
            base.Awake();

            _buildService = new MapBuildService();
            _itemsService = new ItemsMouseService();

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
            IsOverUI = EventSystem.current.IsPointerOverGameObject();
            
            if (!Manager || !Manager.MapIsPresented || Manager.MapIsBeingBuilt) return;

            CheckMouseOverWall();
            _itemsService.CheckMouseOverItem();

            ValidateClicks();
            cameraService.HandleMouseMovement();

            if (IsOverUI)
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

            if (Input.GetMouseButtonDown(0)) ProcessMouseButtonDown(0);
            if (Input.GetMouseButtonDown(1)) ProcessMouseButtonDown(1);
            if (Input.GetMouseButton(0)) ProcessMouseButton(0);
            if (Input.GetMouseButton(1)) ProcessMouseButton(1);

            if (Input.GetMouseButtonUp(0)) RefreshMousePosition();

            if (!LeftClickExpired && Input.GetMouseButtonUp(0))
            {
                ProcessMouseButtonUp(0);
            }
            else
            {
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
            if (Manager.WorkMode is not EWorkMode.Walls || EditorUIManager.Instance.IsAnyObjectEdited) return;

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
                MousePositionOnPlane = mousePlanePosition;
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
            
            SetCursorByCurrentType();
        }

        private void ResolveCommonSimpleModePosition(bool isNullTile)
        {
            if (!_workModesForSimpleNullTileDetection.Contains(Manager.WorkMode)) return;

            GridPositionType = isNullTile ? EGridPositionType.NullTile : EGridPositionType.EditableTile;
        }

        internal void SetCursorToCameraMovement() => SetCursor(ECursorType.Move);

        private bool IsPositionOccupied(Vector3Int newGridPosition)
        {
            return Manager.MapBuilder.GetPrefabByGridPosition(newGridPosition);
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