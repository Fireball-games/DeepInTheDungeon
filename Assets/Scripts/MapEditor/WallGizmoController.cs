using System.Collections.Generic;
using Scripts.EventsManagement;
using Scripts.Helpers;
using Scripts.Helpers.Extensions;
using Scripts.System;
using Scripts.UI.EditorUI;
using UnityEngine;
using static Scripts.Building.Tile.TileDescription;
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

        private EditorMouseService Mouse => EditorMouseService.Instance;
        private MapEditorManager Manager => MapEditorManager.Instance;
        
        private EWallType wallType;
        private Vector3Int currentMousePosition;
        private Dictionary<ETileDirection, PositionRotation> _wallRotationMap;
        private PositionRotation _wallData;
        private bool _isWallPlacementValid;

        private void Awake()
        {
            body.SetActive(false);

            _wallData = new PositionRotation();
            
            _wallRotationMap = new Dictionary<ETileDirection, PositionRotation>
            {
                { ETileDirection.North, new PositionRotation
                {
                    Position = new Vector3(-0.5f, 0, 0f), 
                    Rotation = Quaternion.Euler(Vector3.zero)
                }}, 
                { ETileDirection.East, new PositionRotation
                {
                    Position = new Vector3(0f, 0f, 0.5f), 
                    Rotation = Quaternion.Euler(new Vector3(0, 90, 0))
                }}, 
                { ETileDirection.South, new PositionRotation
                {
                    Position = new Vector3(0.5f, 0f, 0f), 
                    Rotation = Quaternion.Euler(Vector3.zero)
                }}, 
                { ETileDirection.West, new PositionRotation
                {
                    Position = new Vector3(0f, 0f, -0.5f), 
                    Rotation = Quaternion.Euler(new Vector3(0, 90, 0))
                }},
            };
        }

        private void OnEnable()
        {
            EditorEvents.OnWorkModeChanged += OnWorkModeChanged;
            EditorEvents.OnMouseGridPositionChanged += OnMouseGridPositionChanged;
        }

        private void Update()
        {
            if (_isWallPlacementValid && Input.GetMouseButtonUp(0) && !Mouse.LeftClickExpired)
            {
                _wallData.Position = wall.transform.position;
                _wallData.Rotation = wall.transform.localRotation;
                
                EditorUIManager.Instance.OpenTileEditorWindow(wallType, _wallData);
            }
        }

        private void OnDisable()
        {
            EditorEvents.OnWorkModeChanged -= OnWorkModeChanged;
            EditorEvents.OnMouseGridPositionChanged -= OnMouseGridPositionChanged;
        }
        
        public void OnGizmoEntered(ETileDirection direction)
        {
            _isWallPlacementValid = true;
            wall.SetActive(true);

            PositionRotation positionData = _wallRotationMap[direction];

            wall.transform.localPosition = positionData.Position;
            wall.transform.localRotation = positionData.Rotation;

            wallType = EWallType.Between;
            
            if (direction == ETileDirection.North && Manager.EditedLayout.ByGridV3Int(currentMousePosition + GeneralExtensions.GridNorth) == null
                || direction == ETileDirection.East && Manager.EditedLayout.ByGridV3Int(currentMousePosition + GeneralExtensions.GridEast) == null
                || direction == ETileDirection.South && Manager.EditedLayout.ByGridV3Int(currentMousePosition + GeneralExtensions.GridSouth) == null
                || direction == ETileDirection.West && Manager.EditedLayout.ByGridV3Int(currentMousePosition + GeneralExtensions.GridWest) == null
                )
            {
                wallType = EWallType.OnWall;
            }
        }

        public void OnGizmoExited()
        {
            _isWallPlacementValid = false;
            wall.SetActive(false);
        }

        private void OnMouseGridPositionChanged(Vector3Int currentGridPosition, Vector3Int previousGridPosition)
        {
            if (!EditorUIManager.Instance.IsAnyObjectEdited
                && Manager.EditedLayout.HasIndex(currentGridPosition) 
                && Manager.EditedLayout.ByGridV3Int(currentGridPosition) != null 
                && Manager.WorkMode is EWorkMode.Walls)
            {
                body.transform.position = currentGridPosition.ToWorldPosition();

                currentMousePosition = currentGridPosition;
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
            if (workMode is not EWorkMode.Walls)
            {
                body.SetActive(false);
            }
        }
    }
}
