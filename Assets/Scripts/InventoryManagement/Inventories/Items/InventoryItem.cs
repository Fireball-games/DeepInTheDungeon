using System.Collections.Generic;
using Scripts.Building.ItemSpawning;
using Scripts.System.Pooling;
using UnityEngine;

namespace Scripts.InventoryManagement.Inventories.Items
{
    /// <summary>
    /// A ScriptableObject that represents any item that can be put in an
    /// inventory.
    /// </summary>
    /// <remarks>
    /// In practice, you are likely to use a subclass such as `ActionItem` or
    /// `EquipableItem`.
    /// </remarks>
    public abstract class InventoryItem : MapObject
    {
        [Tooltip("Item description to be displayed in UI.")]
        [SerializeField][TextArea] private string description;
        [Tooltip("If true, multiple items of this type can be stacked in the same inventory slot.")]
        [SerializeField] private bool stackable;
        [SerializeField] private ItemModifier[] modifiers;
        
        public ItemModifier[] Modifiers => modifiers;

        private static Dictionary<string, InventoryItem> _itemLookupCache;


        /// <summary>
        /// Spawn the pickup GameObject into the world.
        /// </summary>
        /// <param name="position">Where to spawn the pickup.</param>
        /// <param name="number">How many instances of the item does the pickup represent.</param>
        /// <returns>Reference to the pickup object spawned.</returns>
        public Pickup SpawnPickup(Vector3 position, int number)
        {
            Pickup newPickup = pickup.GetFromPool(ItemSpawner.Parent);
            newPickup.transform.position = position;
            newPickup.Setup(this, number);
            return newPickup;
        }

        public Sprite GetIcon()
        {
            return icon;
        }

        public bool IsStackable()
        {
            return stackable;
        }

        public string GetDescription()
        {
            return description;
        }
    }
}
