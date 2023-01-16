using System.Collections.Generic;
using System.Linq;
using Scripts.Building.PrefabsSpawning.Configurations;
using Scripts.EventsManagement;
using Scripts.Helpers;
using Scripts.Helpers.Extensions;
using Scripts.MapEditor.Services;
using Scripts.System;
using Scripts.UI.EditorUI;
using UnityEngine;
using static Scripts.Building.Tile.TileDescription;
using static Scripts.Enums;
using static Scripts.MapEditor.Enums;

namespace Scripts.MapEditor
{
    public class WallGizmoController : MonoBehaviour
    {
        [SerializeField] private GameObject body;
        [SerializeField] private WallGizmo northGizmo;
        [SerializeField] private WallGizmo eastGizmo;
        [SerializeField] private WallGizmo southGizmo;
        [SerializeField] private WallGizmo westGizmo;
        [SerializeField] private GameObject wall;

        private static EditorMouseService Mouse => EditorMouseService.Instance;
        private static MapEditorManager Manager => MapEditorManager.Instance;

        private EPrefabType _prefabType;
        private EEffectedWalls _effectedWalls = EEffectedWalls.Both;
        private EWorkMode _workMode = EWorkMode.Build;
        private Vector3Int _currentMousePosition;
        private Dictionary<ETileDirection, PositionRotation> _wallRotationMap;
        private PositionRotation _wallData;
        private bool _isWallPlacementValid;
        private bool _isWallAlreadyExisting;
        private bool _isActive;

        private enum EEffectedWalls
        {
            Wall = 1,
            Between = 2,
            Both = 3,
        }

        private void Awake()
        {
            body.SetActive(false);
            _isActive = false;

            _wallData = new PositionRotation();

            _wallRotationMap = new Dictionary<ETileDirection, PositionRotation>
            {
                {
                    ETileDirection.North, new PositionRotation
                    {
                        Position = new Vector3(-0.5f, 0, 0f),
                        Rotation = Quaternion.Euler(new Vector3(0, 180, 0))
                    }
                },
                {
                    ETileDirection.East, new PositionRotation
                    {
                        Position = new Vector3(0f, 0f, 0.5f),
                        Rotation = Quaternion.Euler(new Vector3(0, 270, 0))
                    }
                },
                {
                    ETileDirection.South, new PositionRotation
                    {
                        Position = new Vector3(0.5f, 0f, 0f),
                        Rotation = Quaternion.Euler(Vector3.zero)
                    }
                },
                {
                    ETileDirection.West, new PositionRotation
                    {
                        Position = new Vector3(0f, 0f, -0.5f),
                        Rotation = Quaternion.Euler(new Vector3(0, 90, 0))
                    }
                },
            };
        }

        private void OnEnable()
        {
            EditorEvents.OnWorkModeChanged += OnWorkModeChanged;
            EditorEvents.OnMouseGridPositionChanged += OnMouseGridPositionChanged;
        }

        private void Update()
        {
            if (!_isActive) return;

            HandleMouseclick();
            HandleMouseOverGizmos();
        }

        private void OnDisable()
        {
            EditorEvents.OnWorkModeChanged -= OnWorkModeChanged;
            EditorEvents.OnMouseGridPositionChanged -= OnMouseGridPositionChanged;
        }

        public void Reset()
        {
            SetGizmosActive(true);
        }

        private void HandleMouseclick()
        {
            if (!_isWallPlacementValid || !Input.GetMouseButtonUp(0) || Mouse.LeftClickExpired) return;

            SetGizmosActive(false);

            if (_isWallAlreadyExisting) return;

            _wallData.Position = wall.transform.position;
            _wallData.Rotation = wall.transform.localRotation;

            wall.SetActive(false);

            EditorUIManager.Instance.OpenEditorWindow(_prefabType, _wallData);
        }

        private void HandleMouseOverGizmos()
        {
            if (!EditorUIManager.Instance) return;
            
            if (!EditorUIManager.Instance.isAnyObjectEdited && LayersManager.CheckRayHit(LayersManager.WallGizmoMaskName, out GameObject hitGizmo))
            {
                ETileDirection direction = hitGizmo.GetComponent<WallGizmo>().direction;

                OnGizmoEntered(direction);

                return;
            }

            OnGizmoExited();
        }

        private void OnGizmoEntered(ETileDirection direction)
        {
            wall.SetActive(false);

            PositionRotation positionData = _wallRotationMap[direction];

            wall.transform.localPosition = positionData.Position;
            wall.transform.localRotation = positionData.Rotation;

            _isWallPlacementValid = false;
            _prefabType = EPrefabType.WallBetween;

            if (_effectedWalls == EEffectedWalls.Both || _effectedWalls == EEffectedWalls.Wall)
            {
                if (direction == ETileDirection.North && 
                    Manager.EditedLayout.ByGridV3Int(_currentMousePosition + GeneralExtensions.GridNorth) == null
                    || direction == ETileDirection.East &&
                    Manager.EditedLayout.ByGridV3Int(_currentMousePosition + GeneralExtensions.GridEast) == null
                    || direction == ETileDirection.South &&
                    Manager.EditedLayout.ByGridV3Int(_currentMousePosition + GeneralExtensions.GridSouth) == null
                    || direction == ETileDirection.West &&
                    Manager.EditedLayout.ByGridV3Int(_currentMousePosition + GeneralExtensions.GridWest) == null
                   )
                {
                    _prefabType = _workMode switch
                    {
                        EWorkMode.Walls => EPrefabType.WallOnWall,
                        EWorkMode.Triggers => EPrefabType.Trigger,
                        _ => EPrefabType.Invalid
                    };
                    
                    _isWallPlacementValid = true;
                    wall.SetActive(true);
                }
            }

            if (_workMode is EWorkMode.Walls)
            {
                if (Manager.MapBuilder.GetPrefabConfigurationsOnWorldPosition<WallConfiguration>(wall.transform.position).Count() > 0)
                {
                    _isWallPlacementValid = false;
                    _isWallAlreadyExisting = true;
                    wall.SetActive(false);
                    return;
                }
                
                _isWallPlacementValid = true;
                _isWallAlreadyExisting = false;
            }

            if (_effectedWalls == EEffectedWalls.Both || _effectedWalls == EEffectedWalls.Between)
            {
                _isWallPlacementValid = true;
                wall.SetActive(true);
            }
        }

        private void OnGizmoExited()
        {
            _isWallPlacementValid = false;
            SetGizmosActive(true);
            wall.SetActive(false);
        }

        private void OnMouseGridPositionChanged(Vector3Int currentGridPosition, Vector3Int previousGridPosition)
        {
            wall.SetActive(false);

            if (!EditorUIManager.Instance.isAnyObjectEdited
                && Manager.EditedLayout.HasIndex(currentGridPosition)
                && Manager.EditedLayout.ByGridV3Int(currentGridPosition) != null)
            {
                body.transform.position = currentGridPosition.ToWorldPosition();

                _currentMousePosition = currentGridPosition;
                wall.SetActive(false);
                body.SetActive(true);
            }
            else
            {
                body.SetActive(false);
                _isWallPlacementValid = false;
            }
        }

        private void SetGizmosActive(bool areActive)
        {
            northGizmo.SetActive(areActive);
            eastGizmo.SetActive(areActive);
            southGizmo.SetActive(areActive);
            westGizmo.SetActive(areActive);
        }

        private void OnWorkModeChanged(EWorkMode workMode)
        {
            _workMode = workMode;
            _isActive = false;

            if (workMode != EWorkMode.Walls && workMode != EWorkMode.Triggers)
            {
                body.SetActive(false);
            }
            else
            {
                if (workMode == EWorkMode.Walls)
                {
                    _effectedWalls = EEffectedWalls.Both;
                    _isActive = true;
                }
                else
                {
                    _effectedWalls = EEffectedWalls.Wall;
                    _isActive = true;
                }
            }
        }
    }
}