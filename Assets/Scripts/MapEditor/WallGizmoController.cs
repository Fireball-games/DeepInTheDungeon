using System.Collections.Generic;
using Scripts.EventsManagement;
using Scripts.Helpers.Extensions;
using UnityEngine;
using static Scripts.Building.Tile.TileDescription;
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

        private Dictionary<ETileDirection, Quaternion> _wallRotationMap;

        private EditorMouseService Mouse => EditorMouseService.Instance;

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
            EditorEvents.OnMouseGridPositionChanged += OnMouseGridPositionChanged;
        }

        private void OnDisable()
        {
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
            if (Mouse.GridPositionType is Enums.EGridPositionType.EditableTile &&
                MapEditorManager.Instance.WorkMode is Enums.EWorkMode.Walls)
            {
                body.SetActive(true);
                body.transform.position = currentGridPosition.ToWorldPosition();
            }
        }
    }
}
