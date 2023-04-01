using System.Collections;
using Scripts.Building.ItemSpawning;
using Scripts.Helpers;
using Scripts.Helpers.Extensions;
using Scripts.Player;
using Scripts.System;
using Scripts.UI.EditorUI.PrefabEditors.ItemEditing;
using UnityEngine;
using UnityEngine.Events;
using static Scripts.MapEditor.Enums;
using Logger = Scripts.Helpers.Logger;

namespace Scripts.MapEditor.Services
{
    public class ItemsMouseService
    {
        private const float MouseDeltaToAcceptDragging = 0;
        private const float RotationSpeed = 1000;
        private static EditorMouseService MouseService => EditorMouseService.Instance;
        private static MapEditorManager Manager => MapEditorManager.Instance;
        private static MapObjectInstance ActiveMapObject { get; set; }
        private static MapObjectInstance MouseOverMapObject { get; set; }
        private static ItemCursor ItemCursor => ItemCursor.Instance;
        
        private Vector3 _lastMousePosition;
        private float _activeObjectOriginalHeight;
        private Rigidbody _activeObjectRigidbody;
        
        public static UnityEvent<MapObjectInstance> OnMouseEnterMapObject { get; } = new();
        public static UnityEvent<MapObjectInstance> OnMouseExitMapObject { get; } = new();
        public static UnityEvent<MapObjectInstance> OnObjectActivated { get; } = new();
        public static UnityEvent<MapObjectInstance> OnObjectDeactivated { get; } = new();
        public static UnityEvent<MapObjectInstance> OnObjectChanged { get; } = new();

        private static Vector3 Position 
        {
            get => ActiveMapObject.position;
            set => ActiveMapObject.position = value;
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
                ActiveMapObject = MouseOverMapObject;
                _activeObjectRigidbody = ActiveMapObject.GetComponent<Rigidbody>();
                ItemCursor.Highlight(true);
            }
            else
            {
                ActiveMapObject = null;
                _activeObjectRigidbody = null;
                ItemCursor.Hide();
                return;
            }
            
            CoroutineRunner.Run(ClickGracePeriodCoroutine());
        }
        
        internal void OnMouseButton(int mouseButton)
        {
            if (!_isMouseDownGracePeriodOver) return;

            if (!ActiveMapObject)
            {
                EditorCameraService.CanManipulateView = true;
                return;
            }
            
            EditorCameraService.CanManipulateView = false;
            
            MouseCursorManager.SetCursor(MouseCursorManager.ECursorType.Hidden);
            
            if (mouseButton == 0)
            {
                OnDrag();
            }
            else if (mouseButton == 1)
            {
                OnRotate();
            }
        }
        
        internal void OnMouseButtonUp(int button)
        {
            _isRotating = false;
            
            if (_activeObjectRigidbody)
                _activeObjectRigidbody.constraints = RigidbodyConstraints.None;
            
            ItemCursor.Highlight(false);
            
            if (_isDragging) OnEndDrag();
            
            EditorCameraService.CanManipulateView = true;
            MouseCursorManager.SetCursor(MouseCursorManager.ECursorType.Default);
        }
        
        private void OnMouseEnter()
        {
            OnMouseEnterMapObject.Invoke(MouseOverMapObject);
            
            if (!ActiveMapObject)
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
            OnMouseExitMapObject.Invoke(MouseOverMapObject);
            
            if (!ActiveMapObject) ItemCursor.Hide();
            
            MouseOverMapObject = null;
        }

        private void OnBeginDrag()
        {
            _lastValidDragPosition = Position;
            _activeObjectOriginalHeight = Position.y;
            _activeObjectRigidbody.freezeRotation = true;
        }
        
        private void OnBeginRotate()
        {
            _activeObjectOriginalHeight = Position.y;
            ActiveMapObject.position = Position.SetY(_activeObjectOriginalHeight + PlayerInventoryManager.ItemEditCursorOffset.y);
            _activeObjectRigidbody.constraints = RigidbodyConstraints.FreezePositionY;
        }

        private void OnDrag()
        {
            if (!IsActuallyDragging()) return;
            
            if (IsPositionValid)
            {
                OnObjectChanged.Invoke(ActiveMapObject);
                _isDragging = true;
                Position = MouseService.MousePositionOnPlane.SetY(Position.y);
                ItemCursor.WithOffset(PlayerInventoryManager.ItemEditCursorOffset).WithPosition(Position);
                _lastValidDragPosition = Position;
            }
            else if (_isDragging)
            {
                Position = _lastValidDragPosition.SetY(_activeObjectOriginalHeight);
            }
        }
        
        private void OnRotate()
        {
            float moveInX = Input.GetAxis(Strings.MouseXAxis);
            float moveInY = Input.GetAxis(Strings.MouseYAxis);
            
            if (moveInX != 0 && moveInY != 0) 
            {
                OnObjectChanged.Invoke(ActiveMapObject);
                
                ActiveMapObject.transform.Rotate(Vector3.up, moveInX * RotationSpeed * Time.deltaTime);
                ActiveMapObject.transform.Rotate(Vector3.forward, moveInY * RotationSpeed * Time.deltaTime);
                
                ItemCursor.WithOffset(PlayerInventoryManager.ItemEditCursorOffset).WithPosition(Position);
                
                if (!_isRotating) OnBeginRotate();
                
                _isRotating = true;
            }
        }
        
        private bool IsActuallyDragging()
        {
            if (!MouseService.GetMousePlanePosition(out Vector3 newPosition)) return false;
            
            _lastMousePosition = newPosition;

            bool didMouseMovedEnough = Input.GetAxis(Strings.MouseXAxis) >= MouseDeltaToAcceptDragging 
                                       || Input.GetAxis(Strings.MouseYAxis) >= MouseDeltaToAcceptDragging;
            
            if (didMouseMovedEnough && !_isDragging)
            {
                OnBeginDrag();
            }
            
            return didMouseMovedEnough;
        }

        private void OnEndDrag()
        {
            _isDragging = false;
            
            if (!ActiveMapObject) return;
            
            Position = IsPositionValid ? Position : _lastValidDragPosition;
            _activeObjectRigidbody.freezeRotation = false;
        }

        private bool IsPositionValid
            => ActiveMapObject 
               && Manager.WorkMode is EWorkMode.Items
               && MouseService.GetGridPositionTypeOnMousePosition(_lastMousePosition) is EGridPositionType.EditableTile;
        
        private IEnumerator ClickGracePeriodCoroutine()
        {
            _isMouseDownGracePeriodOver = false;
            yield return new WaitForSeconds(0.1f);
            _isMouseDownGracePeriodOver = true;
        }
    }
}