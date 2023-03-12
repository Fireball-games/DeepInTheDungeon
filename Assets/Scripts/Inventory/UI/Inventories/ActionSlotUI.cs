using Scripts.Inventory.Inventories;
using Scripts.Inventory.Inventories.Items;
using Scripts.Inventory.Utils.UI.Dragging;
using UnityEngine;

namespace Scripts.Inventory.UI.Inventories
{
    /// <summary>
    /// The UI slot for the player action bar.
    /// </summary>
    public class ActionSlotUI : MonoBehaviour, IItemHolder, IDragContainer<InventoryItem>
    {
        // CONFIG DATA
        [SerializeField] private InventoryItemIcon icon;
        [SerializeField] private int index;

        // CACHE
        private ActionStore _store;

        // LIFECYCLE METHODS
        private void Awake()
        {
            _store = GameObject.FindGameObjectWithTag("Player").GetComponent<ActionStore>();
            _store.storeUpdated += UpdateIcon;
        }

        // PUBLIC

        public void AddItems(InventoryItem item, int number)
        {
            _store.AddAction(item, index, number);
        }

        public InventoryItem GetItem()
        {
            return _store.GetAction(index);
        }

        public int GetNumber()
        {
            return _store.GetNumber(index);
        }

        public int MaxAcceptable(InventoryItem item)
        {
            return _store.MaxAcceptable(item, index);
        }

        public void RemoveItems(int number)
        {
            _store.RemoveItems(index, number);
        }

        // PRIVATE

        private void UpdateIcon()
        {
            icon.SetItem(GetItem(), GetNumber());
        }
    }
}
