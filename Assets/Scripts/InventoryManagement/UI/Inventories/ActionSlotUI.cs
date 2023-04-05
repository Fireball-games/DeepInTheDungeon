using System;
using Scripts.InventoryManagement.Inventories;
using Scripts.InventoryManagement.Inventories.Items;
using Scripts.InventoryManagement.Utils.UI.Dragging;
using Scripts.Player;
using UnityEngine;

namespace Scripts.InventoryManagement.UI.Inventories
{
    /// <summary>
    /// The UI slot for the player action bar.
    /// </summary>
    public class ActionSlotUI : MonoBehaviour, IItemHolder, IDragContainer<InventoryItem>
    {
        [SerializeField] private int index;
        private InventoryItemIcon _icon;

        private PlayerController Player => PlayerController.Instance;
        
        private ActionStore _store;

        private void OnEnable()
        {
            _store = Player.InventoryManager.ActionStore;
            _store.storeUpdated.AddListener(UpdateIcon);
            _icon ??= GetComponentInChildren<InventoryItemIcon>();
        }

        public void AddItem(InventoryItem item, int number)
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

        private void UpdateIcon()
        {
            _icon.SetItem(GetItem(), GetNumber());
        }
    }
}
