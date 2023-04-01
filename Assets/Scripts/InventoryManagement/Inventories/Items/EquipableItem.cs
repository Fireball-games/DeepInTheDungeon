using UnityEngine;

namespace Scripts.InventoryManagement.Inventories.Items
{
    /// <summary>
    /// An inventory item that can be equipped to the player. Weapons could be a
    /// subclass of this.
    /// </summary>
    [CreateAssetMenu(menuName = ("Items/Equipable Item"))]
    public class EquipableItem : InventoryItem
    {
        [Tooltip("Where are we allowed to put this item.")]
        [SerializeField]
        private EquipLocation allowedEquipLocation = EquipLocation.WeaponLeft;

        public EquipLocation GetAllowedEquipLocation() => allowedEquipLocation;
    }
}