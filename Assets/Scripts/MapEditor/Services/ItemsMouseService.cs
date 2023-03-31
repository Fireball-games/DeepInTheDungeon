using System.Collections;
using Scripts.Building.ItemSpawning;
using Scripts.EventsManagement;
using Scripts.Helpers;
using Scripts.Helpers.Extensions;
using Scripts.Player;
using Scripts.UI.EditorUI.PrefabEditors.ItemEditing;
using UnityEngine;
using static Scripts.MapEditor.Enums;
using Logger = Scripts.Helpers.Logger;

namespace Scripts.MapEditor.Services
{
    public class ItemsMouseService
    {
        private static EditorMouseService MouseService => EditorMouseService.Instance;
        private static MapEditorManager Manager => MapEditorManager.Instance;

        private static MapObjectInstance ActivatedMapObject { get; set; }
        private static MapObjectInstance MouseOverMapObject { get; set; }
        
        private static ItemCursor ItemCursor => ItemCursor.Instance;

        private static Vector3 Position 
        {
            get => ActivatedMapObject.position;
            set => ActivatedMapObject.position = value;
        }

        private Vector3 _lastValidDragPosition;
        
        private bool _isDragging;
        private bool _isRotating;
        private bool _isMouseDownGracePeriodOver;

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

        internal void OnMouseButtonDown(int button)
        {
            if (MouseOverMapObject)
            {
                ActivatedMapObject = MouseOverMapObject;
                ItemCursor.Highlight(true);
            }
            else
            {
                ActivatedMapObject = null;
                ItemCursor.Hide();
                return;
            }
            
            CoroutineRunner.Run(ClickGracePeriodCoroutine());
        }
        
        internal void OnMouseButton(int mouseButton)
        {
            if (!_isMouseDownGracePeriodOver) return;
            
            EditorCameraService.CanManipulateView = false;
            
            if (mouseButton == 0 && ActivatedMapObject)
            {
                OnDrag();
            }
            else if (mouseButton == 1)
            {
                // TODO: Rotate
            }
        }
        
        internal void OnMouseButtonUp(int button)
        {
            _isRotating = false;
            ItemCursor.Highlight(false);
            
            if (_isDragging) OnEndDrag();
            
            EditorCameraService.CanManipulateView = true;
            
            CoroutineRunner.Stop(ClickGracePeriodCoroutine());
        }
        
        private void OnMouseEnter()
        {
            EditorEvents.TriggerOnMouseEnterMapObject(MouseOverMapObject);
            Logger.Log($"Mouse {"enter".WrapInColor(Colors.LightBlue)} {MouseOverMapObject.Item.DisplayName.WrapInColor(Colors.Yellow)}");
            if (!ActivatedMapObject)
            {
                ActivateItemCursor();
            }
            MouseOverMapObject = MouseOverMapObject;
        }
        
        private static void ActivateItemCursor()
        {
            ItemCursor.WithOffset(PlayerInventoryManager.ItemEditCursorOffset).Show(MouseOverMapObject.position);
            ItemCursor.SetDetailImage(ItemEditor.EditCursorSetup);
        }

        private static void OnMouseExit()
        {
            EditorEvents.TriggerOnMouseExitMapObject(MouseOverMapObject);
            Logger.Log($"Mouse {"exit".WrapInColor(Colors.Orange)} {MouseOverMapObject.Item.DisplayName.WrapInColor(Colors.Yellow)}");
            if (!ActivatedMapObject) ItemCursor.Hide();
            MouseOverMapObject = null;
        }

        private void OnBeginDrag()
        {
            _lastValidDragPosition = Position;
        }

        private void OnDrag()
        {
            if (!IsActuallyDragging()) return;
            
            if (IsPositionValid)
            {
                _isDragging = true;
                Position = MouseService.MousePositionOnPlane.SetY(Position.y);
                ItemCursor.WithPosition(Position).WithOffset(PlayerInventoryManager.ItemEditCursorOffset);
                _lastValidDragPosition = Position;
            }
            else if (_isDragging)
            {
                Position = _lastValidDragPosition;
            }
        }
        
        private bool IsActuallyDragging()
        {
            if (!MouseService.GetMousePlanePosition(out Vector3 _)) return false;
            
            if (!_isDragging)
            {
                OnBeginDrag();
            }

            return Input.GetAxis(Strings.MouseXAxis) != 0 || Input.GetAxis(Strings.MouseYAxis) != 0;
        }

        private void OnEndDrag()
        {
            _isDragging = false;
            if (ActivatedMapObject) Position = IsPositionValid ? Position : _lastValidDragPosition;
        }

        private bool IsPositionValid
            => ActivatedMapObject 
               && Manager.WorkMode is EWorkMode.Items
               && MouseService.GridPositionType is EGridPositionType
                   .EditableTile;
        
        private IEnumerator ClickGracePeriodCoroutine()
        {
            _isMouseDownGracePeriodOver = false;
            yield return new WaitForSeconds(0.1f);
            _isMouseDownGracePeriodOver = true;
        }
    }
}