using Scripts.Building.ItemSpawning;
using Scripts.Helpers.Extensions;
using Scripts.InventoryManagement.Inventories;
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
        private readonly Vector3 _draggedObjectOffset = new(0, 0, 0.7f);

        private Vector3 _startPosition;
        private Transform _originalParent;
        private IDragSource<T> _source;

        private Canvas _parentCanvas;
        private CanvasGroup _canvasGroup;

        private GameObject _draggedItem;
        private static float ThrowPower => DragHelper.ThrowPower;
        private static float MaxDragHeight => DragHelper.MaxDragHeight;
        private static float MinDragHeight => DragHelper.MinDragHeight;
        private static float MaxDragWidth => DragHelper.MaxDragWidth;
        private static float MinDragWidth => DragHelper.MinDragWidth;

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
            GetComponent<RectTransform>().sizeDelta = DragHelper.DragSize;
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
                    Logger.Log($"ThrowForce: {GetThrowForce(_draggedItem.transform.position.y)}");
                    SetCanvasGroupAlpha(0);
                }
                else if (_source.GetItem() is InventoryItem inventoryItem)
                {
                    Player.InventoryManager.SetPickupColliderActive(false);
                    _draggedItem = inventoryItem.SpawnPickup(GetMouseScreenPosition(), 1, false).gameObject;
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

            Pickup spawnedPickup = _draggedItem.GetComponent<Pickup>();

            if (_source.GetItem() is InventoryItem item)
            {
                spawnedPickup = item.SpawnPickup(_draggedItem.transform.position, _draggedItem.transform.rotation, _source.GetNumber());
            }

            if (spawnedPickup)
            {
                Throw(spawnedPickup, GetThrowForce(spawnedPickup.transform.position.y));
            }

            RemoveDraggedItem();

            return null;
        }
        
        private static float GetThrowForce(float objectY)
        {
            // TODO: tune it so throw height is what feels above waist
            // if item has height from floor 0.5 or lower, it will drop, if its higher, it will throw it with every 0.1 of height above 0.5 more far away
            float height = objectY - (Player.transform.position.y);
            Logger.Log($"height: {height}");
            return height > 0 ? height * ThrowPower : 0;
        }

        private void Throw(MapObjectInstance spawnedPickup, float throwForce)
        {
            Rigidbody rb = spawnedPickup.GetComponent<Rigidbody>();

            if (!rb) return;

            Vector3 direction = spawnedPickup.transform.forward;
            float force = throwForce * 100; // Adjust this multiplier to control the strength of the throw
            rb.AddForce(direction * force, ForceMode.Impulse);
        }

        private void RemoveDraggedItem()
        {
            if (!_draggedItem) return;

            _draggedItem.DismissToPool();
            _draggedItem = null;
        }

        private void SetToMousePosition(Transform targetTransform)
        {
            Vector3 mouseScreenPosition = GetMouseScreenPosition();
            // TODO: Try to implement grabbable for dragged object or just clean it and clamp to horizontal axis as well
            mouseScreenPosition = mouseScreenPosition.SetY(Mathf.Clamp(mouseScreenPosition.y,Player.transform.position.y + MinDragHeight, Player.transform.position.y + MaxDragHeight));
            mouseScreenPosition = mouseScreenPosition.SetZ(Mathf.Clamp(mouseScreenPosition.x, Player.transform.position.x + MinDragWidth, Player.transform.position.x + MaxDragWidth));
            // Logger.Log($"object height: {mouseScreenPosition.y.ToString().WrapInColor(Color.cyan)}");
            // targetTransform.position = GetMouseScreenPosition();
            targetTransform.position = mouseScreenPosition;
            targetTransform.localRotation = Quaternion.identity;
        }

        /// <summary>
        /// Gets position of dragged object relative to mouse position relative to the camera.
        /// </summary>
        /// <returns></returns>
        private Vector3 GetMouseScreenPosition()
        {
            return CameraManager.Instance.mainCamera.ScreenToWorldPoint(Input.mousePosition + _draggedObjectOffset);
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