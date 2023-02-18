using System;
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
    public class TileGizmoController : MonoBehaviour
    {
        [SerializeField] private GameObject body;
        [SerializeField] private GameObject centerGizmo;
        [SerializeField] private TileGizmo northGizmo;
        [SerializeField] private TileGizmo eastGizmo;
        [SerializeField] private TileGizmo southGizmo;
        [SerializeField] private TileGizmo westGizmo;
        [SerializeField] private GameObject wallPlaceholder;
        [SerializeField] private GameObject centerPlaceholder;

        private static EditorMouseService Mouse => EditorMouseService.Instance;
        private static MapEditorManager Manager => MapEditorManager.Instance;

        private EPrefabType _prefabType;
        private EAffectedPart _affectedParts = EAffectedPart.None;
        private EWorkMode _workMode = EWorkMode.Build;
        private Vector3Int _currentMousePosition;
        private Dictionary<ETileDirection, PositionRotation> _wallRotationMap;
        private PositionRotation _wallData;
        private bool _isPlacementValid;
        private bool _isWallAlreadyExisting;
        private bool _isActive;

        [Flags]
        private enum EAffectedPart
        {
            None = 0,
            Wall = 1 << 1,
            Between = 1 << 2,
            Floor = 1 << 3,
        }

        private void Awake()
        {
            body.SetActive(false);
            SetGizmosActive(false, true);
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
                {
                    ETileDirection.Floor, new PositionRotation
                    {
                        Position = new Vector3(0f, 0f, 0f),
                        Rotation = Quaternion.Euler(Vector3.zero)
                    }
                }
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
            if (!_isPlacementValid || !Input.GetMouseButtonUp(0) || Mouse.LeftClickExpired) return;

            SetGizmosActive(false);

            if (_isWallAlreadyExisting) return;

            _wallData.Position = wallPlaceholder.transform.position;
            _wallData.Rotation = wallPlaceholder.transform.localRotation;

            SetPlaceholdersActive(false);

            EditorUIManager.Instance.OpenEditorWindow(_prefabType, _wallData);
        }

        private void HandleMouseOverGizmos()
        {
            if (!EditorUIManager.Instance) return;
            
            if (!EditorUIManager.Instance.IsAnyObjectEdited && LayersManager.CheckRayHit(LayersManager.WallGizmoMaskName, out GameObject hitGizmo))
            {
                ETileDirection direction = hitGizmo.GetComponent<TileGizmo>().direction;

                OnGizmoEntered(direction);

                return;
            }

            OnGizmoExited();
        }

        private void OnGizmoEntered(ETileDirection direction)
        {
            SetPlaceholdersActive(false);
            
            PositionRotation positionData = _wallRotationMap[direction];

            wallPlaceholder.transform.localPosition = positionData.Position;
            wallPlaceholder.transform.localRotation = positionData.Rotation;

            _isPlacementValid = false;
            _prefabType = _workMode switch
            {
                EWorkMode.Walls => EPrefabType.WallBetween,
                _ => EPrefabType.Invalid
            };

            if (_affectedParts.HasFlag(EAffectedPart.Wall))
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
                        EWorkMode.Triggers => EPrefabType.TriggerOnWall,
                        _ => EPrefabType.Invalid
                    };
                    
                    _isPlacementValid = true;
                    wallPlaceholder.SetActive(true);
                }
            }

            if (_workMode is EWorkMode.Walls)
            {
                if (Manager.MapBuilder.GetPrefabConfigurationsOnWorldPosition<WallConfiguration>(wallPlaceholder.transform.position).Count() > 0)
                {
                    _isPlacementValid = false;
                    _isWallAlreadyExisting = true;
                    SetPlaceholdersActive(false);
                    return;
                }
                
                _isPlacementValid = true;
                _isWallAlreadyExisting = false;
            }

            if (_affectedParts.HasFlag(EAffectedPart.Between) && direction is not ETileDirection.Floor)
            {
                _prefabType = _workMode switch
                {
                    _ => _prefabType
                };
                
                _isPlacementValid = true;
                wallPlaceholder.SetActive(true);
            }
            
            if (_affectedParts.HasFlag(EAffectedPart.Floor) && direction is ETileDirection.Floor)
            {
                if (_workMode is EWorkMode.Triggers)
                {
                    _prefabType = EPrefabType.TriggerTile;    
                }
                
                _isPlacementValid = true;
                centerPlaceholder.SetActive(true);
            }
        }

        private void OnGizmoExited()
        {
            _isPlacementValid = false;
            SetGizmosActive(true);
            SetPlaceholdersActive(false);
        }

        private void OnMouseGridPositionChanged(Vector3Int currentGridPosition, Vector3Int previousGridPosition)
        {
            SetPlaceholdersActive(false);

            if (!EditorUIManager.Instance.IsAnyObjectEdited
                && Manager.EditedLayout.HasIndex(currentGridPosition)
                && Manager.EditedLayout.ByGridV3Int(currentGridPosition) != null)
            {
                body.transform.position = currentGridPosition.ToWorldPosition();

                _currentMousePosition = currentGridPosition;
                SetPlaceholdersActive(false);
                body.SetActive(true);
            }
            else
            {
                body.SetActive(false);
                _isPlacementValid = false;
            }
        }

        private void SetGizmosActive(bool areActive, bool overrideAffectedParts = false)
        {
            if (_affectedParts.HasFlag(EAffectedPart.Floor) || overrideAffectedParts)
            {
                centerGizmo.SetActive(areActive);
            }

            if (_affectedParts.HasFlag(EAffectedPart.Wall) || _affectedParts.HasFlag(EAffectedPart.Between) || overrideAffectedParts)
            {
                northGizmo.SetActive(areActive);
                eastGizmo.SetActive(areActive);
                southGizmo.SetActive(areActive);
                westGizmo.SetActive(areActive);
            }
        }
        
        private void SetPlaceholdersActive(bool areActive)
        {
            wallPlaceholder.SetActive(areActive);
            centerPlaceholder.SetActive(areActive);
        }

        private void OnWorkModeChanged(EWorkMode workMode)
        {
            _workMode = workMode;
            _isActive = false;
            SetGizmosActive(false, true);

            if (workMode != EWorkMode.Walls && workMode != EWorkMode.Triggers)
            {
                body.SetActive(false);
            }
            else
            {
                if (workMode == EWorkMode.Walls)
                {
                    _affectedParts = EAffectedPart.Wall | EAffectedPart.Between;
                    _isActive = true;
                }
                if (workMode == EWorkMode.Triggers)
                {
                    _affectedParts = EAffectedPart.Wall | EAffectedPart.Floor;
                    _isActive = true;
                }
            }
        }
    }
}