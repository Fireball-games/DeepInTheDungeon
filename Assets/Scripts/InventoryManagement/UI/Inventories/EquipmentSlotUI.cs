using Scripts.InventoryManagement.Inventories;
using Scripts.InventoryManagement.Inventories.Items;
using Scripts.InventoryManagement.Utils.UI.Dragging;
using UnityEngine;

namespace Scripts.InventoryManagement.UI.Inventories
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
            GameObject player = GameObject.FindGameObjectWithTag("Player");
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
            EquipableItem equipableItem = item as EquipableItem;
            if (equipableItem == null) return 0;
            if (equipableItem.GetAllowedEquipLocation() != equipLocation) return 0;
            if (GetItem() != null) return 0;

            return 1;
        }

        public void AddItems(InventoryItem item, int number)
        {
            _playerEquipment.AddItem(equipLocation, (EquipableItem) item);
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