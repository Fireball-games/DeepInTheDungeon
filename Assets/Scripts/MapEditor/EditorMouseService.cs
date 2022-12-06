using System.Collections;
using Scripts.Building.Tile;
using Scripts.EventsManagement;
using Scripts.Helpers;
using Scripts.System;
using UnityEngine;
using static Scripts.MapEditor.Enums;
using Logger = Scripts.Helpers.Logger;

namespace Scripts.MapEditor
{
    public class EditorMouseService : SingletonNotPersisting<EditorMouseService>
    {
        [SerializeField] private Texture2D digCursor;
        [SerializeField] private Texture2D demolishCursor;
        
        public Vector3Int MouseGridPosition => _lastGridPosition;
        public EGridPositionType GridPositionType { get; private set; } = EGridPositionType.None;

        private Plane _layerPlane;
        private Vector3Int _lastGridPosition;
        private WaitForSecondsRealtime _refreshPeriod;
        private readonly Vector2 _defaultMouseHotspot = Vector2.zero;
        private Vector2 _demolishMouseHotspot;

        protected void Awake()
        {
            _layerPlane = new Plane(Vector3.up, new Vector3(0f, 0.5f, 0f));
            _lastGridPosition = new Vector3Int(-1000, -1000, -1000);
            _refreshPeriod = new WaitForSecondsRealtime(0.1f);

            _demolishMouseHotspot = new Vector2(demolishCursor.width / 2, demolishCursor.height / 2);
        }

        private void OnEnable()
        {
            EditorEvents.OnNewMapCreated += OnNewMapCreated;
        }

        private void OnDisable()
        {
            EditorEvents.OnNewMapCreated -= OnNewMapCreated;
        }

        private void OnNewMapCreated() => StartCoroutine(UpdateMousePositionCoroutine());

        private IEnumerator UpdateMousePositionCoroutine()
        {
            while (true)
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
                
                yield return _refreshPeriod;
            }
        }

        private void OnMouseGridPositionChanged(Vector3Int newPosition)
        {
            TileDescription[,] layout = GameController.Instance.CurrentMap.Layout;
            
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

            if (MapEditorManager.Instance.WorkMode == EWorkMode.Build)
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
    }
}
