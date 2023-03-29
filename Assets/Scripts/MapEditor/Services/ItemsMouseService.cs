using Scripts.Building.ItemSpawning;
using Scripts.EventsManagement;
using Scripts.Helpers;
using Scripts.Helpers.Extensions;
using Scripts.Player;
using Scripts.UI.EditorUI.PrefabEditors.ItemEditing;
using UnityEngine;
using UnityEngine.EventSystems;
using static Scripts.MapEditor.Enums;
using Logger = Scripts.Helpers.Logger;

namespace Scripts.MapEditor.Services
{
    public class ItemsMouseService
    {
        private static EditorMouseService MouseService => EditorMouseService.Instance;
        private static MapEditorManager Manager => MapEditorManager.Instance;
        
        public static MapObjectInstance ActivatedMapObject { get; private set; }
        public static MapObjectInstance MouseOverMapObject { get; set; }
        
        private static ItemCursor ItemCursor => ItemCursor.Instance;

        private static Vector3 Position 
        {
            get => MouseOverMapObject.position;
            set => MouseOverMapObject.position = value;
        }

        private Vector3 _lastValidPosition;
        private bool _isDragging;

        internal void CheckMouseOverItem()
        {
            if (Manager.WorkMode is not EWorkMode.Items 
                || (Manager.EditMode is not EEditMode.Edit && Manager.EditMode is not EEditMode.Remove)) return;
            
            if (LayersManager.CheckRayHit(LayersManager.ItemMaskName, out GameObject hit))
            {
                MapObjectInstance mapObject = hit.GetComponent<MapObjectInstance>();
                
                if (mapObject is null)
                {
                    MouseOverMapObject = null;
                    return;
                }

                if (mapObject != MouseOverMapObject)
                {
                    MouseOverMapObject = mapObject;
                    OnMouseEnter();
                }
            }
            else if (MouseOverMapObject)
            {
                OnMouseExit();
            }
        }
        
        private void OnMouseEnter()
        {
            EditorEvents.TriggerOnMouseEnterMapObject(MouseOverMapObject);
            Logger.Log($"Mouse {"enter".WrapInColor(Colors.LightBlue)} {MouseOverMapObject.Item.DisplayName.WrapInColor(Colors.Yellow)}");
            ActivateItemCursor();
            MouseOverMapObject = MouseOverMapObject;
        }
        
        private void ActivateItemCursor()
        {
            ItemCursor.WithOffset(PlayerInventoryManager.ItemEditCursorOffset).Show(MouseOverMapObject.position);
            ItemCursor.SetDetailImage(ItemEditor.EditCursorSetup);
        }

        public static void OnMouseExit()
        {
            EditorEvents.TriggerOnMouseExitMapObject(MouseOverMapObject);
            Logger.Log($"Mouse {"exit".WrapInColor(Colors.Orange)} {MouseOverMapObject.Item.DisplayName.WrapInColor(Colors.Yellow)}");
            ItemCursor.Hide();
            MouseOverMapObject = null;
        }

        private void OnMouseDown()
        {
            ActivatedMapObject = MouseOverMapObject;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _lastValidPosition = Position;
        }

        public void OnDrag()
        {
            if (IsPositionValid)
            {
                _isDragging = true;
                Position = EditorMouseService.Instance.MousePositionOnPlane.SetY(Position.y);
                ActivateItemCursor();
                _lastValidPosition = Position;
            }
            else if (_isDragging)
            {
                Position = _lastValidPosition;
            }
        }

        public void OnEndDrag()
        {
            _isDragging = false;
            Position = IsPositionValid ? Position : _lastValidPosition;
        }

        private bool IsPositionValid
            => ActivatedMapObject == MouseOverMapObject
               && MapEditorManager.Instance.WorkMode is EWorkMode.Items
               && EditorMouseService.Instance.GridPositionType is EGridPositionType
                   .EditableTile;
    }
}