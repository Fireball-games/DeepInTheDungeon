using System.Collections.Generic;
using System.Linq;
using Scripts.Building.Walls.Configurations;
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

        private static EditorMouseService Mouse => EditorMouseService.Instance;
        private static MapEditorManager Manager => MapEditorManager.Instance;

        private Cursor3D _cursor3D;
        
        private EWallType _wallType;
        private readonly Vector3 _cursor3DScale = new(0.11f, 1.1f, 1.1f);
        private Vector3Int _currentMousePosition;
        private Dictionary<ETileDirection, PositionRotation> _wallRotationMap;
        private PositionRotation _wallData;
        private bool _isWallPlacementValid;
        private bool _isWallAlreadyExisting;
        private WallConfiguration _existingConfiguration;

        private void Awake()
        {
            body.SetActive(false);

            _cursor3D = FindObjectOfType<Cursor3D>();

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
                if (!_isWallAlreadyExisting)
                {
                    _wallData.Position = wall.transform.position;
                    _wallData.Rotation = wall.transform.localRotation;
                
                    EditorUIManager.Instance.OpenTileEditorWindow(_wallType, _wallData);
                    return;
                }
                
                EditorUIManager.Instance.OpenTileEditorWindow(_existingConfiguration);
            }
        }

        private void OnDisable()
        {
            EditorEvents.OnWorkModeChanged -= OnWorkModeChanged;
            EditorEvents.OnMouseGridPositionChanged -= OnMouseGridPositionChanged;
        }

        internal void OnGizmoEntered(ETileDirection direction)
        {
            _isWallPlacementValid = true;

            PositionRotation positionData = _wallRotationMap[direction];

            wall.transform.localPosition = positionData.Position;
            wall.transform.localRotation = positionData.Rotation;

            _wallType = EWallType.Between;
            
            if (direction == ETileDirection.North && Manager.EditedLayout.ByGridV3Int(_currentMousePosition + GeneralExtensions.GridNorth) == null
                || direction == ETileDirection.East && Manager.EditedLayout.ByGridV3Int(_currentMousePosition + GeneralExtensions.GridEast) == null
                || direction == ETileDirection.South && Manager.EditedLayout.ByGridV3Int(_currentMousePosition + GeneralExtensions.GridSouth) == null
                || direction == ETileDirection.West && Manager.EditedLayout.ByGridV3Int(_currentMousePosition + GeneralExtensions.GridWest) == null
                )
            {
                _wallType = EWallType.OnWall;
            }

            PrefabConfiguration existingConfiguration =
                Manager.MapBuilder.MapDescription.PrefabConfigurations
                    .FirstOrDefault(c => c.TransformData.Position == wall.transform.position);

            if (existingConfiguration is WallConfiguration configuration)
            {
                _isWallAlreadyExisting = true;
                _existingConfiguration = configuration;
                _cursor3D.ShowAt(wall.transform.position, _cursor3DScale, wall.transform.rotation);
                return;
            }

            _isWallAlreadyExisting = false;
            _existingConfiguration = null;
            _cursor3D.Hide();
            wall.SetActive(true);
        }

        internal void OnGizmoExited()
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

                _currentMousePosition = currentGridPosition;
                body.SetActive(true);
            }
            else
            {
                body.SetActive(false);
                _isWallPlacementValid = false;
            }
        }

        // private void SetGizmosActive(bool areActive)
        // {
        //     northGizmo.SetActive(areActive);
        //     eastGizmo.SetActive(areActive);
        //     southGizmo.SetActive(areActive);
        //     westGizmo.SetActive(areActive);
        // }

        private void OnWorkModeChanged(EWorkMode workMode)
        {
            if (workMode is not EWorkMode.Walls)
            {
                body.SetActive(false);
            }
        }
    }
}