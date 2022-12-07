using Scripts.Building.Tile;
using Scripts.EventsManagement;
using Scripts.Helpers;
using Scripts.System;
using UnityEngine;
using UnityEngine.EventSystems;
using static Scripts.MapEditor.Enums;

namespace Scripts.MapEditor
{
    public class EditorMouseService : SingletonNotPersisting<EditorMouseService>
    {
        [SerializeField] private Texture2D digCursor;
        [SerializeField] private Texture2D demolishCursor;
        private static MapEditorManager Manager => MapEditorManager.Instance;

        public Vector3Int MouseGridPosition => _lastGridPosition;
        public EGridPositionType GridPositionType { get; private set; } = EGridPositionType.None;
        public Vector3Int LastGridMouseDownPosition { get; private set; }

        private Plane _layerPlane;
        private Vector3Int _lastGridPosition;
        private readonly Vector2 _defaultMouseHotspot = Vector2.zero;
        private Vector2 _demolishMouseHotspot;

        protected override void Awake()
        {
            base.Awake();

            _layerPlane = new Plane(Vector3.up, new Vector3(0f, 0.5f, 0f));
            _lastGridPosition = new Vector3Int(-1000, -1000, -1000);

            _demolishMouseHotspot = new Vector2(demolishCursor.width / 2, demolishCursor.height / 2);
        }
        
        private void Update()
        {
            if (Manager.MapIsPresented && !EventSystem.current.IsPointerOverGameObject())
            {
                Vector3Int newGridPosition = Extensions.Vector3IntZero;

                Ray ray = CameraManager.Instance.mainCamera.ScreenPointToRay(Input.mousePosition);
                if (_layerPlane.Raycast(ray, out float distance))
                {
                    newGridPosition = ray.GetPoint(distance).ToVector3Int();
                }

                if (!newGridPosition.Equals(_lastGridPosition))
                {
                    EditorEvents.TriggerOnMouseGridPositionChanged(newGridPosition, _lastGridPosition);
                    OnMouseGridPositionChanged(newGridPosition);
                    _lastGridPosition = newGridPosition;
                }
            }
        }

        public void SetLastMouseDownPosition() => LastGridMouseDownPosition = _lastGridPosition;
        
        private void OnMouseGridPositionChanged(Vector3Int newPosition)
        {
            TileDescription[,] layout = GameManager.Instance.CurrentMap.Layout;

            if (!layout.HasIndex(newPosition.x, newPosition.z))
            {
                Cursor.SetCursor(null, _defaultMouseHotspot, CursorMode.Auto);
                GridPositionType = EGridPositionType.None;
                return;
            }

            bool isNullTile = layout[newPosition.x, newPosition.z] == null;

            GridPositionType = isNullTile ? EGridPositionType.Null : EGridPositionType.EditableTile;
            // Logger.Log($"GridPositionType: {GridPositionType}");
            Texture2D newCursor = null;
            Vector2 hotspot = _defaultMouseHotspot;

            if (Manager.WorkMode == EWorkMode.Build)
            {
                newCursor = isNullTile ? digCursor : demolishCursor;
                hotspot = isNullTile ? _defaultMouseHotspot : _demolishMouseHotspot;
            }

            SetCursor(newCursor, hotspot);
        }

        private void SetCursor(Texture2D image, Vector3 hotspot)
        {
            Cursor.SetCursor(image, hotspot, CursorMode.Auto);
        }

        public void RefreshMousePosition() => OnMouseGridPositionChanged(MouseGridPosition);

        public void ResetCursor()
        {
            SetCursor(null, Vector3.zero);
        }
    }
}