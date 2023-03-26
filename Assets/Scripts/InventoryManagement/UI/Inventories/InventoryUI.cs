using Scripts.Helpers.Extensions;
using Scripts.InventoryManagement.Inventories;
using Scripts.System.MonoBases;
using Scripts.System.Pooling;
using UnityEngine;

namespace Scripts.InventoryManagement.UI.Inventories
{
    /// <summary>
    /// To be placed on the root of the inventory UI. Handles spawning all the
    /// inventory slot prefabs.
    /// </summary>
    public class InventoryUI : UIElementBase
    {
        [SerializeField] private InventorySlotUI inventoryItemPrefab;

        private Inventory _playerInventory;
        private Transform _itemsParent;

        public void ToggleOpen() => SetActive(!body.activeSelf);
        
        private void Awake() 
        {
            _playerInventory = Inventory.GetPlayerInventory();
            _itemsParent = transform.Find("Background/Frame/ScrollView/Viewport/Content");
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

        private void Redraw()
        {
            _itemsParent.gameObject.DismissAllChildrenToPool();

            for (int i = 0; i < _playerInventory.GetSize(); i++)
            {
                InventorySlotUI itemUI = inventoryItemPrefab.GetFromPool(_itemsParent);
                itemUI.Setup(_playerInventory, i);
            }
        }
    }
}