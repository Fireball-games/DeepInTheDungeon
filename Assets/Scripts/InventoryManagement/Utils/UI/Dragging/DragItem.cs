using Scripts.InventoryManagement.Inventories.Items;
using Scripts.Player;
using Scripts.System.MonoBases;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;
using UnityEngine.EventSystems;

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
        where T : class
    {
        Vector3 startPosition;
        Transform originalParent;
        IDragSource<T> source;

        Canvas parentCanvas;

        private void Awake()
        {
            parentCanvas = GetComponentInParent<Canvas>();
            source = GetComponentInParent<IDragSource<T>>();
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            startPosition = transform.position;
            originalParent = transform.parent;
            GetComponent<CanvasGroup>().blocksRaycasts = false;
            // Set the parent to the canvas so that it's rendered on top of other objects.
            transform.SetParent(parentCanvas.transform, true);
            GetComponent<RectTransform>().sizeDelta = PlayerInventoryManager.DragSize;
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            transform.position = eventData.position;
            
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                // TODO: continue here, ths does not work
                if (source.GetItem() is InventoryItem inventoryItem)
                {
                    //Spawn item pickup on mouse position 0.2 in front of camera
                    Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    mousePos.z = 0.2f;
                    InventoryItem pickup = Instantiate(inventoryItem, mousePos, Quaternion.identity);
                }
                
                Helpers.Logger.Log("Not over game object");
            }
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            transform.position = startPosition;
            GetComponent<CanvasGroup>().blocksRaycasts = true;
            transform.SetParent(originalParent, true);

            IDragDestination<T> container = !EventSystem.current.IsPointerOverGameObject() 
                ? parentCanvas.GetComponent<IDragDestination<T>>() 
                : GetContainer(eventData);

            if (container != null)
            {
                DropItemIntoContainer(container);
            }
        }

        private IDragDestination<T> GetContainer(PointerEventData eventData)
        {
            if (!eventData.pointerEnter) return null;
            
            IDragDestination<T> container = eventData.pointerEnter.GetComponentInParent<IDragDestination<T>>();

            return container;
        }

        private void DropItemIntoContainer(IDragDestination<T> destination)
        {
            if (ReferenceEquals(destination, source)) return;

            // Swap won't be possible
            if (destination is not IDragContainer<T> destinationContainer 
                || source is not IDragContainer<T> sourceContainer 
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
            T draggingItem = source.GetItem();
            int draggingNumber = source.GetNumber();

            int acceptable = destination.MaxAcceptable(draggingItem);
            int toTransfer = Mathf.Min(acceptable, draggingNumber);

            if (toTransfer <= 0) return true;
            
            source.RemoveItems(toTransfer);
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