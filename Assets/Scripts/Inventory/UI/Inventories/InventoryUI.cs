using System;
using UnityEngine;

namespace Scripts.Inventory.UI.Inventories
{
    /// <summary>
    /// To be placed on the root of the inventory UI. Handles spawning all the
    /// inventory slot prefabs.
    /// </summary>
    public class InventoryUI : MonoBehaviour
    {
        // CONFIG DATA
        [SerializeField] InventorySlotUI InventoryItemPrefab;

        // CACHE
        Inventory.Inventories.Inventory playerInventory;

        // LIFECYCLE METHODS

        private void Awake() 
        {
            playerInventory = Inventory.Inventories.Inventory.GetPlayerInventory();
        }

        private void OnEnable()
        {
            playerInventory.OnInventoryUpdated.AddListener(Redraw);
        }

        private void Start()
        {
            Redraw();
        }
        
        private void OnDisable()
        {
            playerInventory.OnInventoryUpdated.RemoveListener(Redraw);
        }

        // PRIVATE

        private void Redraw()
        {
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }

            for (int i = 0; i < playerInventory.GetSize(); i++)
            {
                var itemUI = Instantiate(InventoryItemPrefab, transform);
                itemUI.Setup(playerInventory, i);
            }
        }
    }
}