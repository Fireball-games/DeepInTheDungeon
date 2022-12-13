using System.Collections.Generic;
using Scripts.Building.Tile;
using Scripts.EventsManagement;
using Scripts.Helpers;
using Scripts.Helpers.Extensions;
using Scripts.UI.EditorUI;
using UnityEngine;
using static Scripts.Building.Tile.TileDescription;
using static Scripts.MapEditor.Enums;
using Logger = Scripts.Helpers.Logger;
using NotImplementedException = System.NotImplementedException;

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

        internal EWallType wallType;

        private Vector3Int currentMousePosition;

        private Dictionary<ETileDirection, Quaternion> _wallRotationMap;

        private EditorMouseService Mouse => EditorMouseService.Instance;
        private MapEditorManager Manager => MapEditorManager.Instance;
        private bool _isWallPlacementValid;

        private void Awake()
        {
            body.SetActive(false);
            
            _wallRotationMap = new Dictionary<ETileDirection, Quaternion>
            {
                { ETileDirection.North, Quaternion.Euler(Vector3.zero) },
                { ETileDirection.East, Quaternion.Euler(new Vector3(0, 90, 0)) },
                { ETileDirection.South, Quaternion.Euler(new Vector3(0, 180, 0)) },
                { ETileDirection.West, Quaternion.Euler(new Vector3(0, 270, 0)) },
            };
        }

        private void OnEnable()
        {
            EditorEvents.OnWorkModeChanged += OnWorkModeChanged;
            EditorEvents.OnMouseGridPositionChanged += OnMouseGridPositionChanged;
        }

        private void Update()
        {
            if (_isWallPlacementValid && Input.GetMouseButtonUp(0))
            {
                EditorUIManager.Instance.OpenTileEditorWindow(wallType, wall.transform.position);
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
            wall.transform.localRotation = _wallRotationMap[direction];

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

        private void SetWallsActive(bool areActive)
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
