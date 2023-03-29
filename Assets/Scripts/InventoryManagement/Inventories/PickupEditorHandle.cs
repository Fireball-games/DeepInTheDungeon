using Scripts.Building.ItemSpawning;
using Scripts.EventsManagement;
using Scripts.Helpers;
using Scripts.Helpers.Extensions;
using Scripts.MapEditor;
using Scripts.MapEditor.Services;
using Scripts.System;
using Scripts.System.MonoBases;
using Scripts.UI.EditorUI.PrefabEditors.ItemEditing;
using UnityEngine;
using UnityEngine.EventSystems;
using static Scripts.MapEditor.Enums;
using Logger = Scripts.Helpers.Logger;

namespace Scripts.InventoryManagement.Inventories
{
    /// <summary>
    /// This script handles the Pickup behavior for editing purposes in the map editor. Is added by Pickup script and destroy itself in play mode.
    /// </summary>
    public class PickupEditorHandle : MonoBase, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public static MapObjectInstance ActivatedMapObject { get; private set; }
        public static MapObjectInstance MouseOverMapObject { get; private set; }
        
        private static ItemCursor ItemCursor => ItemCursor.Instance;

        private MapObjectInstance _pickup;

        private Vector3 _lastValidPosition;
        private bool _isDragging;

        private void OnEnable()
        {
            if (GameManager.IsInPlayMode) Destroy(this);
            _pickup ??= GetComponent<Pickup>();
            _lastValidPosition = position;
        }

        private void OnMouseEnter()
        {
            EditorEvents.TriggerOnMouseEnterMapObject(_pickup);
            Logger.Log($"Mouse enter {_pickup.Item.DisplayName.WrapInColor(Colors.Yellow)}");
            ActivateItemCursor();
            MouseOverMapObject = _pickup;
        }
        
        private void ActivateItemCursor()
        {
            ItemCursor.Show(_pickup.position).WithOffset(Vector3.zero);
            ItemCursor.SetDetailImage(ItemEditor.EditCursorSetup);
        }

        private void OnMouseExit()
        {
            EditorEvents.TriggerOnMouseExitMapObject(_pickup);
            Logger.Log($"Mouse exit {_pickup.Item.DisplayName.WrapInColor(Colors.Yellow)}");
            ItemCursor.Hide();
            MouseOverMapObject = null;
        }

        private void OnMouseDown()
        {
            ActivatedMapObject = _pickup;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _lastValidPosition = position;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (IsPositionValid)
            {
                _isDragging = true;
                position = EditorMouseService.Instance.MousePositionOnPlane.SetY(position.y);
                ActivateItemCursor();
                _lastValidPosition = position;
            }
            else if (_isDragging)
            {
                position = _lastValidPosition;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _isDragging = false;
            position = IsPositionValid ? position : _lastValidPosition;
        }

        private bool IsPositionValid
            => ActivatedMapObject == _pickup
               && MapEditorManager.Instance.WorkMode is EWorkMode.Items
               && EditorMouseService.Instance.GridPositionType is EGridPositionType
                   .EditableTile;
    }
}