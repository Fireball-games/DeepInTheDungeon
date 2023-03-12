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
        [SerializeField] private InventorySlotUI inventoryItemPrefab;

        // CACHE
        private Inventory.Inventories.Inventory _playerInventory;

        // LIFECYCLE METHODS

        private void Awake() 
        {
            _playerInventory = Inventory.Inventories.Inventory.GetPlayerInventory();
        }

        private void OnEnable()
        {
            _playerInventory.OnInventoryUpdated.AddListener(Redraw);
        }

        private void Start()
        {
            Redraw();
        }
        
        private void OnDisable()
        {
            _playerInventory.OnInventoryUpdated.RemoveListener(Redraw);
        }

        // PRIVATE

        private void Redraw()
        {
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }

            for (int i = 0; i < _playerInventory.GetSize(); i++)
            {
                var itemUI = Instantiate(inventoryItemPrefab, transform);
                itemUI.Setup(_playerInventory, i);
            }
        }
    }
}