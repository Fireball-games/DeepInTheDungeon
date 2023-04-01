using Scripts.InventoryManagement.Inventories;
using Scripts.InventoryManagement.Inventories.Items;
using Scripts.InventoryManagement.Utils.UI.Dragging;
using Scripts.Player;
using UnityEngine;

namespace Scripts.InventoryManagement.UI.Inventories
{
    /// <summary>
    /// An slot for the players equipment.
    /// </summary>
    public class EquipmentSlotUI : MonoBehaviour, IItemHolder, IDragContainer<InventoryItem>
    {
        [SerializeField] private InventoryItemIcon icon;
        [SerializeField] private EquipLocation equipLocation = EquipLocation.WeaponLeft;

        private Equipment _playerEquipment;

        private void Awake() 
        { 
            _playerEquipment = PlayerController.Instance.InventoryManager.Equipment;
            _playerEquipment.equipmentUpdated += RedrawUI;
        }

        private void Start() 
        {
            _playerEquipment ??= PlayerController.Instance.InventoryManager.Equipment;
            RedrawUI();
        }

        public int MaxAcceptable(InventoryItem item)
        {
            EquipableItem equipableItem = item as EquipableItem;
            if (equipableItem == null) return 0;
            if (!equipableItem.GetAllowedEquipLocation().HasFlag(equipLocation)) return 0;
            return GetItem() != null ? 0 : 1;
        }

        public void AddItem(InventoryItem item, int number)
        {
            _playerEquipment.AddItem(equipLocation, (EquipableItem) item);
        }

        public InventoryItem GetItem()
        {
            return _playerEquipment.GetItemInSlot(equipLocation);
        }

        public int GetNumber()
        {
            return GetItem() != null ? 1 : 0;
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