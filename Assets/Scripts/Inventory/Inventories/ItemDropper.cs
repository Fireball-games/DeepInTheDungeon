using System;
using System.Collections.Generic;
using Scripts.System.Saving;
using UnityEngine;
using UnityEngine.Serialization;

namespace Scripts.Inventory.Inventories
{
    /// <summary>
    /// To be placed on anything that wishes to drop pickups into the world.
    /// Tracks the drops for saving and restoring.
    /// </summary>
    public class ItemDropper : MonoBehaviour, ISavable
    {
        // STATE
        private List<Pickup> _droppedItems = new();


        private void Awake()
        {
            Guid = "Item Dropper";
        }

        /// <summary>
        /// Create a pickup at the current position.
        /// </summary>
        /// <param name="item">The item type for the pickup.</param>
        /// <param name="number">
        /// The number of items contained in the pickup. Only used if the item
        /// is stackable.
        /// </param>
        public void DropItem(InventoryItem item, int number)
        {
            SpawnPickup(item, GetDropLocation(), number);
        }

        /// <summary>
        /// Create a pickup at the current position.
        /// </summary>
        /// <param name="item">The item type for the pickup.</param>
        public void DropItem(InventoryItem item)
        {
            SpawnPickup(item, GetDropLocation(), 1);
        }

        // PROTECTED

        /// <summary>
        /// Override to set a custom method for locating a drop.
        /// </summary>
        /// <returns>The location the drop should be spawned.</returns>
        protected virtual Vector3 GetDropLocation()
        {
            return transform.position;
        }

        // PRIVATE

        public void SpawnPickup(InventoryItem item, Vector3 spawnLocation, int number)
        {
            Pickup pickup = item.SpawnPickup(spawnLocation, number);
            _droppedItems.Add(pickup);
        }

        [Serializable]
        private struct DropRecord
        {
            public string itemID;
            [FormerlySerializedAs("Position")] public Vector3 position;
            public int number;
        }

        public string Guid { get; set; }

        object ISavable.CaptureState()
        {
            RemoveDestroyedDrops();
            DropRecord[] droppedItemsList = new DropRecord[_droppedItems.Count];
            for (int i = 0; i < droppedItemsList.Length; i++)
            {
                droppedItemsList[i].itemID = _droppedItems[i].GetItem().GetItemID();
                droppedItemsList[i].position = _droppedItems[i].transform.position;
                droppedItemsList[i].number = _droppedItems[i].GetNumber();
            }
            return droppedItemsList;
        }

        void ISavable.RestoreState(object state)
        {
            DropRecord[] droppedItemsList = (DropRecord[])state;
            foreach (DropRecord item in droppedItemsList)
            {
                InventoryItem pickupItem = InventoryItem.GetFromID(item.itemID);
                Vector3 position = item.position;
                int number = item.number;
                SpawnPickup(pickupItem, position, number);
            }
        }

        /// <summary>
        /// Remove any drops in the world that have subsequently been picked up.
        /// </summary>
        private void RemoveDestroyedDrops()
        {
            List<Pickup> newList = new();
            foreach (Pickup item in _droppedItems)
            {
                if (item != null)
                {
                    newList.Add(item);
                }
            }
            _droppedItems = newList;
        }
    }
}