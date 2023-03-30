using Scripts.InventoryManagement.Inventories.Items;
using Scripts.Player;
using Scripts.System;
using Scripts.System.MonoBases;
using Scripts.System.Pooling;
using UnityEngine;
using UnityEngine.EventSystems;
using Logger = Scripts.Helpers.Logger;

namespace Scripts.InventoryManagement.Utils.UI.Dragging
{
    /// <summary>
    /// Allows a UI element to be dragged and dropped from and to a container.
    /// 
    /// Create a subclass for the type you want to be draggable. Then place on
    /// the UI element you want to make draggable.
    /// 
    /// During dragging, the item is re-parented to the parent canvas.
    /// 
    /// After the item is dropped it will be automatically return to the
    /// original UI parent. It is the job of components implementing `IDragContainer`,
    /// `IDragDestination and `IDragSource` to update the interface after a drag
    /// has occurred.
    /// </summary>
    /// <typeparam name="T">The type that represents the item being dragged.</typeparam>
    public class DragItem<T> : MonoBase, IBeginDragHandler, IDragHandler, IEndDragHandler
        where T : MapObject
    {
        private readonly Vector3 _draggedObjectOffset = new(0, 0, 0.6f);
        
        private Vector3 _startPosition;
        private Transform _originalParent;
        private IDragSource<T> _source;

        private Canvas _parentCanvas;
        private CanvasGroup _canvasGroup;

        private GameObject _draggedItem;
        
        private bool IsOverUI => EventSystem.current.IsPointerOverGameObject();
        
        private static PlayerController Player => PlayerController.Instance;

        private void Awake()
        {
            _parentCanvas = GetComponentInParent<Canvas>();
            _source = GetComponentInParent<IDragSource<T>>();
        }

        private void Update()
        {
            if (!_draggedItem || IsOverUI) return;
            
            SetToMousePosition(_draggedItem.transform);
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            _startPosition = transform.position;
            _originalParent = transform.parent;
            GetComponent<CanvasGroup>().blocksRaycasts = false;
            // Set the parent to the canvas so that it's rendered on top of other objects.
            transform.SetParent(_parentCanvas.transform, true);
            GetComponent<RectTransform>().sizeDelta = PlayerInventoryManager.DragSize;
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            transform.position = eventData.position;
            
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                if (_draggedItem)
                {
                    SetToMousePosition(_draggedItem.transform);
                    _draggedItem.SetActive(true);
                    SetCanvasGroupAlpha(0);
                }
                else if (_source.GetItem() is InventoryItem inventoryItem)
                {
                    Player.InventoryManager.SetPickupColliderActive(false);
                    _draggedItem = inventoryItem.SpawnPickup(GetMouseScreenPosition(), 1).gameObject;
                    _draggedItem.transform.SetParent(Player.transform, true);
                }
            }
            else
            {
                if (_draggedItem)
                {
                    _draggedItem.SetActive(false);
                    SetCanvasGroupAlpha(1);
                }
            }
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            transform.position = _startPosition;
            GetComponent<CanvasGroup>().blocksRaycasts = true;
            // TODO: not when not over UI, it drops/throws the object then, but for now...
            SetCanvasGroupAlpha(1);
            transform.SetParent(_originalParent, true);

            IDragDestination<T> container = !EventSystem.current.IsPointerOverGameObject() 
                ? DropObjectIntoWorld()
                : GetContainer(eventData);
            
            RemoveDraggedItem();
            
            if (container != null)
            {
                DropItemIntoContainer(container);
                return;
            }
            
            Player.InventoryManager.SetPickupColliderActive(true);
            _source.RemoveItems(int.MaxValue);
        }

        /// <summary>
        /// Drops new Pickup into the world. Return value is only so this method can be conveniently used in ternary operator.
        /// </summary>
        /// <returns></returns>
        private IDragDestination<T> DropObjectIntoWorld()
        {
            if (!_draggedItem) return null;
            
            if (_source.GetItem() is InventoryItem item) 
            {
                item.SpawnPickup(GetMouseScreenPosition(), _draggedItem.transform.rotation, _source.GetNumber());
            }
                
            RemoveDraggedItem();

            return null;
        }
        
        private void RemoveDraggedItem()
        {
            if (!_draggedItem) return;
            
            _draggedItem.DismissToPool();
            _draggedItem = null;
            Logger.Log("Removed dragged item");
        }

        private void SetToMousePosition(Transform targetTransform)
        {
            targetTransform.position = GetMouseScreenPosition();
            targetTransform.localRotation = Quaternion.identity;
        }

        private Vector3 GetMouseScreenPosition()
        {
            Vector3 mousePos = CameraManager.Instance.mainCamera.ScreenToWorldPoint(Input.mousePosition + _draggedObjectOffset);
            return mousePos;
        }
        
        private void SetCanvasGroupAlpha(float alpha)
        {
            _canvasGroup ??= GetComponent<CanvasGroup>();
            _canvasGroup ??= gameObject.AddComponent<CanvasGroup>();
            _canvasGroup.alpha = alpha;
        }

        private IDragDestination<T> GetContainer(PointerEventData eventData)
        {
            if (!eventData.pointerEnter) return null;
            
            IDragDestination<T> container = eventData.pointerEnter.GetComponentInParent<IDragDestination<T>>();

            return container;
        }

        private void DropItemIntoContainer(IDragDestination<T> destination)
        {
            if (ReferenceEquals(destination, _source)) return;

            // Swap won't be possible
            if (destination is not IDragContainer<T> destinationContainer 
                || _source is not IDragContainer<T> sourceContainer 
                || destinationContainer.GetItem() == null 
                || ReferenceEquals(destinationContainer.GetItem(), sourceContainer.GetItem()))
            {
                AttemptSimpleTransfer(destination);
                return;
            }

            AttemptSwap(destinationContainer, sourceContainer);
        }

        private static void AttemptSwap(IDragContainer<T> destination, IDragContainer<T> source)
        {
            // Provisionally remove item from both sides. 
            int removedSourceNumber = source.GetNumber();
            T removedSourceItem = source.GetItem();
            int removedDestinationNumber = destination.GetNumber();
            T removedDestinationItem = destination.GetItem();

            source.RemoveItems(removedSourceNumber);
            destination.RemoveItems(removedDestinationNumber);

            int sourceTakeBackNumber = CalculateTakeBack(removedSourceItem, removedSourceNumber, source, destination);
            int destinationTakeBackNumber = CalculateTakeBack(removedDestinationItem, removedDestinationNumber, destination, source);

            // Do take backs (if needed)
            if (sourceTakeBackNumber > 0)
            {
                source.AddItem(removedSourceItem, sourceTakeBackNumber);
                removedSourceNumber -= sourceTakeBackNumber;
            }
            if (destinationTakeBackNumber > 0)
            {
                destination.AddItem(removedDestinationItem, destinationTakeBackNumber);
                removedDestinationNumber -= destinationTakeBackNumber;
            }

            // Abort if we can't do a successful swap
            if (source.MaxAcceptable(removedDestinationItem) < removedDestinationNumber ||
                destination.MaxAcceptable(removedSourceItem) < removedSourceNumber ||
                removedSourceNumber == 0)
            {
                if (removedDestinationNumber > 0)
                {
                    destination.AddItem(removedDestinationItem, removedDestinationNumber);
                }
                if (removedSourceNumber > 0)
                {
                    source.AddItem(removedSourceItem, removedSourceNumber);
                }
                return;
            }

            // Do swaps
            if (removedDestinationNumber > 0)
            {
                source.AddItem(removedDestinationItem, removedDestinationNumber);
            }
            if (removedSourceNumber > 0)
            {
                destination.AddItem(removedSourceItem, removedSourceNumber);
            }
        }

        private bool AttemptSimpleTransfer(IDragDestination<T> destination)
        {
            T draggingItem = _source.GetItem();
            int draggingNumber = _source.GetNumber();

            int acceptable = destination.MaxAcceptable(draggingItem);
            int toTransfer = Mathf.Min(acceptable, draggingNumber);

            if (toTransfer <= 0) return true;
            
            _source.RemoveItems(toTransfer);
            destination.AddItem(draggingItem, toTransfer);
            
            return false;
        }

        private static int CalculateTakeBack(T removedItem, int removedNumber, IDragContainer<T> removeSource, IDragContainer<T> destination)
        {
            int takeBackNumber = 0;
            int destinationMaxAcceptable = destination.MaxAcceptable(removedItem);

            if (destinationMaxAcceptable < removedNumber)
            {
                takeBackNumber = removedNumber - destinationMaxAcceptable;

                int sourceTakeBackAcceptable = removeSource.MaxAcceptable(removedItem);

                // Abort and reset
                if (sourceTakeBackAcceptable < takeBackNumber)
                {
                    return 0;
                }
            }
            return takeBackNumber;
        }
    }
}