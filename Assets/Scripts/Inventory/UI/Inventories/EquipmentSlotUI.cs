using Scripts.Inventory.Inventories;
using Scripts.Inventory.Inventories.Items;
using Scripts.Inventory.Utils.UI.Dragging;
using UnityEngine;

namespace Scripts.Inventory.UI.Inventories
{
    /// <summary>
    /// An slot for the players equipment.
    /// </summary>
    public class EquipmentSlotUI : MonoBehaviour, IItemHolder, IDragContainer<InventoryItem>
    {
        // CONFIG DATA

        [SerializeField] private InventoryItemIcon icon;
        [SerializeField] private EquipLocation equipLocation = EquipLocation.Weapon;

        // CACHE
        private Equipment _playerEquipment;

        // LIFECYCLE METHODS
       
        private void Awake() 
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            _playerEquipment = player.GetComponent<Equipment>();
            _playerEquipment.equipmentUpdated += RedrawUI;
        }

        private void Start() 
        {
            RedrawUI();
        }

        // PUBLIC

        public int MaxAcceptable(InventoryItem item)
        {
            if (item == null) return 0;
            if (item.GetAllowedEquipLocation() != equipLocation) return 0;
            return GetItem() != null ? 0 : 1;
        }

        public void AddItems(InventoryItem item, int number)
        {
            _playerEquipment.AddItem(equipLocation, item);
        }

        public InventoryItem GetItem()
        {
            return _playerEquipment.GetItemInSlot(equipLocation);
        }

        public int GetNumber()
        {
            if (GetItem() != null)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        public void RemoveItems(int number)
        {
            _playerEquipment.RemoveItem(equipLocation);
        }

        // PRIVATE

        private void RedrawUI()
        {
            icon.SetItem(_playerEquipment.GetItemInSlot(equipLocation));
        }
    }
}