using System.Collections.Generic;
using Scripts.Building.Tile;
using Scripts.EventsManagement;
using Scripts.Helpers;
using Scripts.Helpers.Extensions;
using UnityEngine;
using static Scripts.Building.Tile.TileDescription;
using static Scripts.MapEditor.Enums;
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

        private Vector3Int currentMousePosition;

        private Dictionary<ETileDirection, Quaternion> _wallRotationMap;

        private EditorMouseService Mouse => EditorMouseService.Instance;
        private MapEditorManager Manager => MapEditorManager.Instance;

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

        private void OnDisable()
        {
            EditorEvents.OnWorkModeChanged -= OnWorkModeChanged;
            EditorEvents.OnMouseGridPositionChanged -= OnMouseGridPositionChanged;
        }
        
        public void OnGizmoEntered(ETileDirection direction)
        {
            wall.SetActive(true);
            wall.transform.localRotation = _wallRotationMap[direction];
        }

        public void OnGizmoExited() => wall.SetActive(false);

        private void OnMouseGridPositionChanged(Vector3Int currentGridPosition, Vector3Int previousGridPosition)
        {
            if (Manager.EditedLayout.HasIndex(currentGridPosition) 
                && Manager.EditedLayout.ByGridV3Int(currentGridPosition) != null 
                && MapEditorManager.Instance.WorkMode is EWorkMode.Walls)
            {
                SetWallsActive(false);
                
                currentMousePosition = currentGridPosition;
                body.SetActive(true);
                body.transform.position = currentGridPosition.ToWorldPosition();

                List<List<List<TileDescription>>> layout = Manager.EditedLayout;

                northGizmo.SetActive(layout.ByGridV3Int(currentGridPosition + GeneralExtensions.GridNorth) != null);
                eastGizmo.SetActive(layout.ByGridV3Int(currentGridPosition + GeneralExtensions.GridEast) != null);
                southGizmo.SetActive(layout.ByGridV3Int(currentGridPosition + GeneralExtensions.GridSouth) != null);
                westGizmo.SetActive(layout.ByGridV3Int(currentGridPosition + GeneralExtensions.GridWest) != null);
            }
            else
            {
                body.SetActive(false);
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
